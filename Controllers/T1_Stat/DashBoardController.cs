using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using T1EsportsWeb.Models.DTO;
using T1EsportsWeb.Models.T1Stat;
using T1EsportsWeb.Models.T1Stat.DTO;

namespace T1EsportsWeb.Controllers
{
    public class DashboardController : Controller
    {
        private readonly T1StatDbContext _context;
        private int? _t1TeamId = null;

        public DashboardController(T1StatDbContext context)
        {
            _context = context;
        }

        private async Task<int> GetT1TeamId()
        {
            if (_t1TeamId.HasValue) return _t1TeamId.Value;
            _t1TeamId = await _context.Teams.Where(t => t.Name == "T1").Select(t => t.IdTeam).FirstOrDefaultAsync();
            return _t1TeamId ?? 0;
        }

        // GET: /Dashboard
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách tournament và đối thủ để đổ vào dropdown
            var tournaments = await _context.Tournaments
                .Where(t => _context.Series.Any(s => s.TournamentId == t.IdTournament))
                .OrderBy(t => t.Year).ThenBy(t => t.Name)
                .Select(t => new { t.IdTournament, t.Name, t.Year })
                .ToListAsync();

            var opponents = await _context.Teams
                .Where(t => t.Name != "T1" && _context.Series.Any(s => s.TeamOpponentId == t.IdTeam))
                .Select(t => new { t.IdTeam, t.Name })
                .ToListAsync();

            ViewBag.Tournaments = tournaments;
            ViewBag.Opponents = opponents;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Overview()
        {
            var t1Id = await GetT1TeamId();
            if (t1Id == 0) return NotFound();

            var seriesList = await _context.Series
                .Where(s => s.TeamT1Id == t1Id)
                .Include(s => s.Tournament)
                .ToListAsync();

            var domesticSeries = seriesList.Count(s => s.Tournament.Region == "KR");
            var internationalSeries = seriesList.Count(s => s.Tournament.Region == "INT");

            var gameTeams = _context.GameTeams.Where(gt => gt.TeamId == t1Id);
            var totalGames = await gameTeams.CountAsync();
            var bo3Count = seriesList.Count(s => s.BestOf == 3);
            var bo5Count = seriesList.Count(s => s.BestOf == 5);
            var bo1Count = seriesList.Count(s => s.BestOf == 1);

            var blueGames = await gameTeams.Where(gt => gt.Side == "Blue").CountAsync();
            var redGames = await gameTeams.Where(gt => gt.Side == "Red").CountAsync();
            var blueWins = await gameTeams.Where(gt => gt.Side == "Blue" && gt.Result == "Win").CountAsync();
            var redWins = await gameTeams.Where(gt => gt.Side == "Red" && gt.Result == "Win").CountAsync();

            return Json(new DashboardOverviewDto
            {
                DomesticSeriesCount = domesticSeries,
                InternationalSeriesCount = internationalSeries,
                TotalGames = totalGames,
                Bo3Count = bo3Count,
                Bo5Count = bo5Count,
                Bo1Count = bo1Count,
                BlueGames = blueGames,
                RedGames = redGames,
                BlueWins = blueWins,
                RedWins = redWins,
                BlueWinRate = blueGames == 0 ? 0 : (double)blueWins / blueGames * 100,
                RedWinRate = redGames == 0 ? 0 : (double)redWins / redGames * 100
            });
        }


        [HttpGet]
        public async Task<IActionResult> SeriesByTournament(
    int? tournamentId,
    int? opponentId,
    DateTime? startDate,
    DateTime? endDate)
        {
            var t1Id = await GetT1TeamId();
            if (t1Id == 0) return NotFound();

            var seriesQuery = _context.Series
                .Where(s => s.TeamT1Id == t1Id);

            if (tournamentId.HasValue)
                seriesQuery = seriesQuery.Where(s => s.TournamentId == tournamentId.Value);
            if (opponentId.HasValue)
                seriesQuery = seriesQuery.Where(s => s.TeamOpponentId == opponentId.Value);
            if (startDate.HasValue)
                seriesQuery = seriesQuery.Where(s => s.MatchDate >= DateOnly.FromDateTime(startDate.Value));
            if (endDate.HasValue)
                seriesQuery = seriesQuery.Where(s => s.MatchDate <= DateOnly.FromDateTime(endDate.Value));

            seriesQuery = seriesQuery.Include(s => s.Tournament);
            var seriesList = await seriesQuery.ToListAsync();

            var result = seriesList
         .GroupBy(s => new { s.Tournament.IdTournament, s.Tournament.Name, s.Tournament.Year, s.Tournament.Region, s.Tournament.IsT1winner })
         .Select(g => new SeriesByTournamentDto
         {
             TournamentName = g.Key.Name,
             Year = g.Key.Year.Value,
             Region = g.Key.Region,
             IsT1Winner = g.Key.IsT1winner == "YES",
             SeriesWon = g.Count(s =>
             {
                 var wins = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Win" && gt.Game.SeriesId == s.IdSeries);
                 var losses = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Loss" && gt.Game.SeriesId == s.IdSeries);
                 return wins > losses;
             }),
             SeriesLost = g.Count(s =>
             {
                 var wins = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Win" && gt.Game.SeriesId == s.IdSeries);
                 var losses = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Loss" && gt.Game.SeriesId == s.IdSeries);
                 return wins < losses;
             }),
             // Tính riêng cho KR/INT
             KRWon = g.Key.Region == "KR" ? g.Count(s =>
             {
                 var wins = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Win" && gt.Game.SeriesId == s.IdSeries);
                 var losses = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Loss" && gt.Game.SeriesId == s.IdSeries);
                 return wins > losses;
             }) : 0,
             KRLost = g.Key.Region == "KR" ? g.Count(s =>
             {
                 var wins = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Win" && gt.Game.SeriesId == s.IdSeries);
                 var losses = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Loss" && gt.Game.SeriesId == s.IdSeries);
                 return wins < losses;
             }) : 0,
             INTWon = g.Key.Region == "INT" ? g.Count(s =>
             {
                 var wins = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Win" && gt.Game.SeriesId == s.IdSeries);
                 var losses = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Loss" && gt.Game.SeriesId == s.IdSeries);
                 return wins > losses;
             }) : 0,
             INTLost = g.Key.Region == "INT" ? g.Count(s =>
             {
                 var wins = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Win" && gt.Game.SeriesId == s.IdSeries);
                 var losses = _context.GameTeams.Count(gt => gt.TeamId == t1Id && gt.Result == "Loss" && gt.Game.SeriesId == s.IdSeries);
                 return wins < losses;
             }) : 0
         })
         .ToList();

            // Tính winrate cho từng tournament
            foreach (var item in result)
            {
                int total = item.SeriesWon + item.SeriesLost;
                item.WinRate = total == 0 ? 0 : (double)item.SeriesWon / total * 100;
            }

            result = result
                   .OrderBy(r => r.IdTournament)
                   .ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GamesByTournament(int? tournamentId, int? opponentId, DateTime? startDate, DateTime? endDate)
        {
            var t1Id = await GetT1TeamId();
            if (t1Id == 0) return NotFound();

            var gamesQuery = _context.GameTeams
                .Where(gt => gt.TeamId == t1Id)
                .Include(gt => gt.Game)
                    .ThenInclude(g => g.Series)
                        .ThenInclude(s => s.Tournament)
                .AsQueryable();

            if (tournamentId.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.TournamentId == tournamentId.Value);
            if (opponentId.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.TeamOpponentId == opponentId.Value);
            if (startDate.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.MatchDate >= DateOnly.FromDateTime(startDate.Value));
            if (endDate.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.MatchDate <= DateOnly.FromDateTime(endDate.Value));

            var games = await gamesQuery.ToListAsync();

            var result = games
                .GroupBy(gt => new { gt.Game.Series.Tournament.IdTournament, gt.Game.Series.Tournament.Name, gt.Game.Series.Tournament.Year, gt.Game.Series.Tournament.Region })
                .Select(g => new GamesByTournamentDto
                {
                    TournamentName = g.Key.Name,
                    Year = g.Key.Year.Value,
                    Region = g.Key.Region,
                    GamesWon = g.Count(gt => gt.Result == "Win"),
                    GamesLost = g.Count(gt => gt.Result == "Loss"),
                    GamesKRWon = g.Key.Region == "KR" ? g.Count(gt => gt.Result == "Win") : 0,
                    GamesKRLost = g.Key.Region == "KR" ? g.Count(gt => gt.Result == "Loss") : 0,
                    GamesINTWon = g.Key.Region == "INT" ? g.Count(gt => gt.Result == "Win") : 0,
                    GamesINTLost = g.Key.Region == "INT" ? g.Count(gt => gt.Result == "Loss") : 0
                })
                .ToList();

            foreach (var item in result)
            {
                int total = item.GamesWon + item.GamesLost;
                item.WinRate = total == 0 ? 0 : (double)item.GamesWon / total * 100;
            }

            result = result.OrderBy(r => r.IdTournament).ToList();
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> WinrateByPatch(
            int? tournamentId,
            int? opponentId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var t1Id = await GetT1TeamId();
            if (t1Id == 0) return NotFound();

            var gamesQuery = _context.GameTeams
                .Where(gt => gt.TeamId == t1Id)
                .Include(gt => gt.Game)
                    .ThenInclude(g => g.Series)
                        .ThenInclude(s => s.Tournament)
                            .AsQueryable();

            if (tournamentId.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.TournamentId == tournamentId.Value);
            if (opponentId.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.TeamOpponentId == opponentId.Value);
            if (startDate.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.MatchDate >= DateOnly.FromDateTime(startDate.Value));
            if (endDate.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.MatchDate <= DateOnly.FromDateTime(endDate.Value));

            var games = await gamesQuery.ToListAsync();

            var result = games
                .GroupBy(gt => gt.Game.Patch)
                .Select(g => new WinrateByPatchDto
                {
                    Patch = g.Key,
                    WinRate = g.Count() == 0 ? 0 : (double)g.Count(gt => gt.Result == "Win") / g.Count() * 100
                })
                .OrderBy(w => w.Patch)
                .ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> PlayerWinrate(string role = null, int? opponentId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var t1Id = await GetT1TeamId();
                if (t1Id == 0) return Json(new List<PlayerWinrateDto>());

                DateOnly? start = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : null;
                DateOnly? end = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : null;

                // Truy vấn gộp tất cả player của T1 trong 1 lần duy nhất
                var query = _context.GamePlayers
                    .Where(gp => gp.GameTeam.TeamId == t1Id);

                // Áp dụng filter
                if (!string.IsNullOrEmpty(role))
                    query = query.Where(gp => gp.Player.Position == role);
                if (opponentId.HasValue)
                    query = query.Where(gp => gp.GameTeam.Game.Series.TeamOpponentId == opponentId);
                if (start.HasValue)
                    query = query.Where(gp => gp.GameTeam.Game.Series.MatchDate >= start);
                if (end.HasValue)
                    query = query.Where(gp => gp.GameTeam.Game.Series.MatchDate <= end);

                var stats = await query
                    .GroupBy(gp => new { gp.Player.IngameName, gp.Player.Position })
                    .Select(g => new PlayerWinrateDto
                    {
                        PlayerName = g.Key.IngameName,
                        Role = g.Key.Position,
                        Games = g.Count(),
                        Wins = g.Count(gp => gp.GameTeam.Result == "Win"),
                        WinRate = (double)g.Count(gp => gp.GameTeam.Result == "Win") / g.Count() * 100
                    })
                    .OrderByDescending(p => p.WinRate)
                    .ToListAsync();

                return Json(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> HeadToHead(
    int? tournamentId,
    int? opponentId,
    DateTime? startDate,
    DateTime? endDate)
        {
            var t1Id = await GetT1TeamId();
            if (t1Id == 0) return NotFound();

            //// *** TẠM THỜI: CHỈ LẤY 2 ĐỘI CỤ THỂ ***
            //// Lấy danh sách các opponent ID cần kiểm tra
            //var targetOpponentIds = new List<int> { 47, 52 }; // Vietnam, AG.AL

            // Lấy tất cả game của T1, kèm series, tournament và đối thủ
            var gamesQuery = _context.GameTeams
                .Where(gt => gt.TeamId == t1Id)
                .Include(gt => gt.Game)
                    .ThenInclude(g => g.Series)
                        .ThenInclude(s => s.Tournament)
                .Include(gt => gt.Game)
                    .ThenInclude(g => g.Series)
                        .ThenInclude(s => s.TeamOpponent)
                .AsQueryable();

            // Áp dụng bộ lọc (giữ nguyên các filter hiện có)
            if (tournamentId.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.TournamentId == tournamentId.Value);
            if (opponentId.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.TeamOpponentId == opponentId.Value);
            if (startDate.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.MatchDate >= DateOnly.FromDateTime(startDate.Value));
            if (endDate.HasValue)
                gamesQuery = gamesQuery.Where(gt => gt.Game.Series.MatchDate <= DateOnly.FromDateTime(endDate.Value));

            //// *** THÊM ĐIỀU KIỆN CHỈ LẤY 2 ĐỘI ***
            //gamesQuery = gamesQuery.Where(gt => targetOpponentIds.Contains(gt.Game.Series.TeamOpponentId));

            var games = await gamesQuery.ToListAsync();

            // Log để kiểm tra
            Console.WriteLine($"Found {games.Count} games for target opponents");
            foreach (var g in games)
            {
                Console.WriteLine($"GameId: {g.GameId}, Opponent: {g.Game.Series.TeamOpponent?.Name}, Result: {g.Result}, Region: {g.Game.Series.Tournament?.Region}");
            }

            // Nhóm theo opponent
            // 1. Kiểm tra lại việc gộp nhóm
            var result = games
                .Where(gt => gt.Game?.Series?.TeamOpponent != null) // Chỉ lấy khi có đối thủ rõ ràng
                .GroupBy(gt => new {
                    Id = gt.Game.Series.TeamOpponentId,
                    Name = gt.Game.Series.TeamOpponent.Name
                })
                .Select(g => {
                    var domestic = g.Where(gt => gt.Game.Series.Tournament?.Region == "KR");
                    var international = g.Where(gt => gt.Game.Series.Tournament?.Region == "INT" || gt.Game.Series.Tournament?.Region == "VN"); // Thêm các region khác nếu cần

                    return new HeadToHeadDto
                    {
                        OpponentName = g.Key.Name,
                        DomesticGames = domestic.Count(),
                        DomesticWins = domestic.Count(gt => gt.Result == "Win"),
                        InternationalGames = international.Count(),
                        InternationalWins = international.Count(gt => gt.Result == "Win")
                    };
                })
                .Select(h => new HeadToHeadDto
                {
                    OpponentName = h.OpponentName,
                    DomesticGames = h.DomesticGames,
                    DomesticWins = h.DomesticWins,
                    DomesticWinRate = h.DomesticGames == 0 ? 0 : Math.Round((double)h.DomesticWins / h.DomesticGames * 100, 1),
                    InternationalGames = h.InternationalGames,
                    InternationalWins = h.InternationalWins,
                    InternationalWinRate = h.InternationalGames == 0 ? 0 : Math.Round((double)h.InternationalWins / h.InternationalGames * 100, 1)
                })
                .ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> ChampionStats(string type, int? tournamentId, int? opponentId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var t1Id = await GetT1TeamId();
                int targetTeamId = (type == "T1") ? t1Id : (opponentId ?? 0);

                DateOnly? start = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : null;
                DateOnly? end = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : null;

                // 1. Lấy danh sách ID các Game thỏa mãn bộ lọc (để dùng cho cả Picks và Bans)
                var filteredGameIds = _context.Games
                    .Where(g => (!tournamentId.HasValue || g.Series.TournamentId == tournamentId) &&
                                (!opponentId.HasValue || g.Series.TeamOpponentId == opponentId) &&
                                (!start.HasValue || g.Series.MatchDate >= start) &&
                                (!end.HasValue || g.Series.MatchDate <= end))
                    .Select(g => g.IdGame);

                // 2. Thống kê Picks từ GamePlayers
                var picksData = await _context.GamePlayers
                    .Where(gp => filteredGameIds.Contains(gp.GameTeam.GameId) && gp.GameTeam.TeamId == targetTeamId)
                    .GroupBy(gp => new { gp.ChampionId, gp.Champion.Name })
                    .Select(g => new {
                        g.Key.ChampionId,
                        g.Key.Name,
                        Picks = g.Count(),
                        Wins = g.Count(gp => gp.GameTeam.Result == "Win")
                    })
                    .ToListAsync();

                // 3. Thống kê Bans (Lấy tất cả lượt ban trong các game đã lọc)
                var bansData = await _context.Bans
                    .Where(b => filteredGameIds.Contains(b.GameId))
                    .GroupBy(b => b.ChampionId)
                    .Select(g => new { ChampionId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.ChampionId, x => x.Count);

                // 4. Kết hợp dữ liệu
                var result = picksData.Select(p => new ChampionStatsDto
                {
                    ChampionName = p.Name ?? "Unknown",
                    Picks = p.Picks,
                    Wins = p.Wins,
                    Bans = bansData.ContainsKey(p.ChampionId) ? bansData[p.ChampionId] : 0,
                    WinRate = p.Picks == 0 ? 0 : (double)p.Wins / p.Picks * 100
                })
                .OrderByDescending(c => c.Picks)
                .ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChampionStats Error: {ex.Message}");
                return StatusCode(500, "Lỗi khi lấy dữ liệu tướng");
            }
        }
    }
}