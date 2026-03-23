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

            // 🚀 TỐI ƯU: Load sẵn Games và GameTeams của T1 ngay từ đầu bằng Include
            var seriesQuery = _context.Series
                .Where(s => s.TeamT1Id == t1Id)
                .Include(s => s.TeamOpponent)
                .Include(s => s.Tournament)
                .Include(s => s.Games) // Load sẵn danh sách Game
                    .ThenInclude(g => g.GameTeams.Where(gt => gt.TeamId == t1Id)) // Chỉ lấy GameTeam của T1
                .AsQueryable();

            if (startDate.HasValue)
                seriesQuery = seriesQuery.Where(s => s.MatchDate >= DateOnly.FromDateTime(startDate.Value));
            if (endDate.HasValue)
                seriesQuery = seriesQuery.Where(s => s.MatchDate <= DateOnly.FromDateTime(endDate.Value));
            if (tournamentId.HasValue)
                seriesQuery = seriesQuery.Where(s => s.TournamentId == tournamentId.Value);

            var seriesList = await seriesQuery.OrderByDescending(s => s.MatchDate).ThenBy(s => s.IdSeries).ToListAsync();

            // Tournament dropdown giữ nguyên logic filter của Boss
            var tournaments = await _context.Tournaments
                .Where(t => _context.Series.Any(s => s.TournamentId == t.IdTournament && s.TeamT1Id == t1Id))
                .OrderByDescending(t => t.Year).Select(t => new { t.IdTournament, t.Name, t.Year }).ToListAsync();

            ViewBag.Tournaments = tournaments;
            ViewBag.SelectedTournament = tournamentId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            // ⚡ Xử lý dữ liệu trên RAM cực nhanh vì đã có sẵn dữ liệu từ câu query trên
            var result = seriesList.Select(series => {
                var gameDtos = series.Games.OrderBy(g => g.GameNumber).Select(g => {
                    var t1Gt = g.GameTeams.FirstOrDefault();
                    return new GameDto
                    {
                        GameId = g.IdGame,
                        GameNumber = g.GameNumber ?? 0,
                        Patch = g.Patch,
                        Result = t1Gt?.Result ?? "N/A",
                        Side = t1Gt?.Side ?? "N/A",
                        Link = g.Link
                    };
                }).ToList();

                int wins = gameDtos.Count(g => g.Result == "Win");
                int losses = gameDtos.Count(g => g.Result == "Loss");

                return new MatchHistoryDto
                {
                    SeriesId = series.IdSeries,
                    MatchDate = series.MatchDate.HasValue ? new DateTime(series.MatchDate.Value.Year, series.MatchDate.Value.Month, series.MatchDate.Value.Day) : DateTime.MinValue,
                    OpponentName = series.TeamOpponent?.Name ?? "Unknown",
                    Result = wins > losses ? "Win" : (losses > wins ? "Loss" : "Draw"),
                    BestOf = series.BestOf,
                    TournamentName = series.Tournament?.Name ?? "Unknown",
                    TournamentYear = series.Tournament?.Year ?? 0,
                    Games = gameDtos
                };
            }).ToList();

            return View(result);
        }

        // GET: /MatchHistory/GetGameDetails?gameId=xxx
        [HttpGet]
        public async Task<IActionResult> GetGameDetails(int gameId)
        {
            var t1Id = await _context.Teams.Where(t => t.Name == "T1").Select(t => t.IdTeam).FirstOrDefaultAsync();
            if (t1Id == 0) return NotFound();

            // 🚀 TỐI ƯU CỰC MẠNH: Dùng Select để lấy trực tiếp dữ liệu thay vì kéo toàn bộ Entity vào RAM
            var t1Lineup = await _context.GamePlayers
                .AsNoTracking()
                .Where(gp => gp.GameTeam.GameId == gameId && gp.GameTeam.TeamId == t1Id)
                .OrderBy(gp => gp.PickOrder)
                .Select(p => new TeamLineupDto
                {
                    PlayerName = p.Player.IngameName,
                    PlayerPhotoUrl = p.Player.PhotoUrl,
                    ChampionName = p.Champion.Name,
                    ChampionImageUrl = p.Champion.ImageUrl
                }).ToListAsync();

            var oppLineup = await _context.GamePlayers
                .AsNoTracking()
                .Where(gp => gp.GameTeam.GameId == gameId && gp.GameTeam.TeamId != t1Id)
                .OrderBy(gp => gp.PickOrder)
                .Select(p => new TeamLineupDto
                {
                    PlayerName = p.Player.IngameName,
                    PlayerPhotoUrl = p.Player.PhotoUrl,
                    ChampionName = p.Champion.Name,
                    ChampionImageUrl = p.Champion.ImageUrl
                }).ToListAsync();

            var bans = await _context.Bans
                .AsNoTracking()
                .Where(b => b.GameId == gameId)
                .Select(b => new BanDto
                {
                    TeamName = b.Team.Name,
                    ChampionName = b.Champion.Name,
                    ChampionImageUrl = b.Champion.ImageUrl
                }).ToListAsync();

            return Json(new
            {
                t1Lineup = t1Lineup,
                opponentLineup = oppLineup,
                bans = bans
            });
        }
    }
}