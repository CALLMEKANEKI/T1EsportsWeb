using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using T1EsportsWeb.Models.T1Stat;
using T1EsportsWeb.Models.T1Stat.DTO;

namespace T1EsportsWeb.Controllers
{
    public class MatchHistoryController : Controller
    {
        private readonly T1StatDbContext _context;

        public MatchHistoryController(T1StatDbContext context)
        {
            _context = context;
        }

        // GET: /MatchHistory
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? tournamentId)
        {
            var t1Id = await _context.Teams.Where(t => t.Name == "T1").Select(t => t.IdTeam).FirstOrDefaultAsync();
            if (t1Id == 0) return View(new List<MatchHistoryDto>());

            // Query series
            var seriesQuery = _context.Series
                .Where(s => s.TeamT1Id == t1Id)
                .Include(s => s.TeamOpponent)
                .Include(s => s.Tournament)
                .AsQueryable();

            // Apply filters
            if (startDate.HasValue)
                seriesQuery = seriesQuery.Where(s => s.MatchDate >= DateOnly.FromDateTime(startDate.Value));
            if (endDate.HasValue)
                seriesQuery = seriesQuery.Where(s => s.MatchDate <= DateOnly.FromDateTime(endDate.Value));
            if (tournamentId.HasValue)
                seriesQuery = seriesQuery.Where(s => s.TournamentId == tournamentId.Value);

            // Order by date descending, then by series id (to have deterministic order)
            seriesQuery = seriesQuery.OrderByDescending(s => s.MatchDate).ThenBy(s => s.IdSeries);

            var seriesList = await seriesQuery.ToListAsync();

            // Build list of tournaments for dropdown (filtered by same date range)
            var tournamentQuery = _context.Tournaments
                .Where(t => _context.Series.Any(s => s.TournamentId == t.IdTournament && s.TeamT1Id == t1Id))
                .AsQueryable();
            if (startDate.HasValue)
                tournamentQuery = tournamentQuery.Where(t => _context.Series.Any(s => s.TournamentId == t.IdTournament && s.MatchDate >= DateOnly.FromDateTime(startDate.Value)));
            if (endDate.HasValue)
                tournamentQuery = tournamentQuery.Where(t => _context.Series.Any(s => s.TournamentId == t.IdTournament && s.MatchDate <= DateOnly.FromDateTime(endDate.Value)));

            var tournaments = await tournamentQuery
                .OrderBy(t => t.Year).ThenBy(t => t.Name)
                .Select(t => new { t.IdTournament, t.Name, t.Year })
                .ToListAsync();

            ViewBag.Tournaments = tournaments;
            ViewBag.SelectedTournament = tournamentId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            // Build DTOs
            var result = new List<MatchHistoryDto>();
            foreach (var series in seriesList)
            {
                // Get games of this series where T1 participated
                var gameTeams = await _context.GameTeams
                    .Where(gt => gt.TeamId == t1Id && gt.Game.SeriesId == series.IdSeries)
                    .Include(gt => gt.Game)
                    .OrderBy(gt => gt.Game.GameNumber)
                    .ToListAsync();

                var gameDtos = gameTeams.Select(gt => new GameDto
                {
                    GameId = gt.Game.IdGame,
                    GameNumber = gt.Game.GameNumber ?? 0,
                    Patch = gt.Game.Patch,
                    Result = gt.Result,
                    Side = gt.Side,
                    Link = gt.Game.Link
                }).ToList();

                int wins = gameTeams.Count(g => g.Result == "Win");
                int losses = gameTeams.Count(g => g.Result == "Loss");
                string seriesResult = wins > losses ? "Win" : (losses > wins ? "Loss" : "Draw");

                result.Add(new MatchHistoryDto
                {
                    SeriesId = series.IdSeries,
                    MatchDate = new DateTime(series.MatchDate.Value.Year, series.MatchDate.Value.Month, series.MatchDate.Value.Day),
                    OpponentName = series.TeamOpponent?.Name ?? "Unknown",
                    Result = seriesResult,
                    BestOf = series.BestOf,
                    TournamentName = series.Tournament?.Name ?? "Unknown",
                    TournamentYear = series.Tournament?.Year ?? 0,
                    Games = gameDtos
                });
            }

            return View(result);
        }

        // GET: /MatchHistory/GetGameDetails?gameId=xxx
        [HttpGet]
        public async Task<IActionResult> GetGameDetails(int gameId)
        {
            var t1Id = await _context.Teams.Where(t => t.Name == "T1").Select(t => t.IdTeam).FirstOrDefaultAsync();
            if (t1Id == 0) return NotFound();

            // Get the two game_team entries for this game
            var gameTeams = await _context.GameTeams
                .Where(gt => gt.GameId == gameId)
                .Include(gt => gt.Team)
                .ToListAsync();

            var t1GameTeam = gameTeams.FirstOrDefault(gt => gt.TeamId == t1Id);
            var oppGameTeam = gameTeams.FirstOrDefault(gt => gt.TeamId != t1Id);

            // Helper function to get lineup
            async Task<List<TeamLineupDto>> GetLineup(int gameTeamId)
            {
                var players = await _context.GamePlayers
                    .Where(gp => gp.GameTeamId == gameTeamId)
                    .Include(gp => gp.Player)
                    .Include(gp => gp.Champion)
                    .OrderBy(gp => gp.PickOrder)
                    .ToListAsync();

                return players.Select(p => new TeamLineupDto
                {
                    PlayerName = p.Player.IngameName,
                    PlayerPhotoUrl = p.Player.PhotoUrl,
                    ChampionName = p.Champion.Name,
                    ChampionImageUrl = p.Champion.ImageUrl
                }).ToList();
            }

            var t1Lineup = t1GameTeam != null ? await GetLineup(t1GameTeam.IdGameTeam) : new List<TeamLineupDto>();
            var oppLineup = oppGameTeam != null ? await GetLineup(oppGameTeam.IdGameTeam) : new List<TeamLineupDto>();

            // Get bans
            var bans = await _context.Bans
                .Where(b => b.GameId == gameId)
                .Include(b => b.Champion)
                .Include(b => b.Team)
                .ToListAsync();

            var banDtos = bans.Select(b => new BanDto
            {
                TeamName = b.Team.Name,
                ChampionName = b.Champion.Name,
                ChampionImageUrl = b.Champion.ImageUrl
            }).ToList();

            return Json(new
            {
                t1Lineup = t1Lineup,
                opponentLineup = oppLineup, // Đổi từ oppLineup sang opponentLineup
                bans = banDtos
            });
        }
    }
}