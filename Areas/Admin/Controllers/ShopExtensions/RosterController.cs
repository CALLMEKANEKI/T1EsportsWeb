using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using T1EsportsWeb.Models;
using T1EsportsWeb.Models.T1Stat.DTO;
using T1EsportsWeb.Models.T1Stat;

namespace T1EsportsWeb.Controllers
{
    public class RosterController : Controller
    {
        private readonly T1StatDbContext _statsContext;
        private readonly IMemoryCache _cache;

        public RosterController(T1StatDbContext statsContext, IMemoryCache cache)
        {
            _statsContext = statsContext;
            _cache = cache;
        }

        // ---------- TRANG INDEX (ĐỘI HÌNH) ----------
        public IActionResult Index()
        {
            const string cacheKey = "T1RosterStatsData_Top18";

            if (!_cache.TryGetValue(cacheKey, out List<PlayerStats>? allPlayerStats) || allPlayerStats == null)
            {
                allPlayerStats = new List<PlayerStats>();

                var t1Team = _statsContext.Teams.FirstOrDefault(t => t.Name == "T1");
                int t1TeamId = t1Team != null ? t1Team.IdTeam : 1;

                // Lấy 18 tuyển thủ đầu tiên (dựa trên IdPlayer)
                var top18Players = _statsContext.Players
                    .OrderBy(p => p.IdPlayer)
                    .Take(18)
                    .ToList();

                var allChampions = _statsContext.Champions.ToList();

                foreach (var player in top18Players)
                {
                    // Lấy các game của player khi họ chơi cho T1 (thông qua GameTeams)
                    var playerGames = (from gp in _statsContext.GamePlayers
                                       join gt in _statsContext.GameTeams on gp.GameTeamId equals gt.IdGameTeam
                                       join g in _statsContext.Games on gt.GameId equals g.IdGame
                                       where gp.PlayerId == player.IdPlayer && gt.TeamId == t1TeamId
                                       select new
                                       {
                                           SeriesId = g.SeriesId,
                                           ChampionId = gp.ChampionId,
                                           Result = gt.Result
                                       }).ToList();

                    // Nếu không có game nào cho T1, có thể bỏ qua hoặc vẫn hiển thị với 0
                    // Tính toán series (BO3/BO5) như cũ
                    var seriesStats = playerGames.GroupBy(x => x.SeriesId)
                        .Select(g => new
                        {
                            IsWin = g.Count(x => x.Result != null && x.Result.Trim() == "Win") >
                                    g.Count(x => x.Result != null && x.Result.Trim() == "Loss")
                        }).ToList();

                    int totalSeries = seriesStats.Count;
                    int totalWins = seriesStats.Count(s => s.IsWin);

                    // Tính top 3 tướng
                    var championCounts = playerGames.GroupBy(g => g.ChampionId)
                        .OrderByDescending(g => g.Count())
                        .Take(3)
                        .Select(g => new { ChampId = g.Key, PlayCount = g.Count() })
                        .ToList();

                    var topChampions = new List<TopChampion>();
                    foreach (var top in championCounts)
                    {
                        var champInfo = allChampions.FirstOrDefault(c => c.IdChampion == top.ChampId);
                        if (champInfo != null)
                        {
                            topChampions.Add(new TopChampion
                            {
                                Name = champInfo.Name,
                                Image = champInfo.ImageUrl,
                                Count = top.PlayCount
                            });
                        }
                    }

                    allPlayerStats.Add(new PlayerStats
                    {
                        PlayerInfo = player,
                        TotalGames = totalSeries,
                        Wins = totalWins,
                        Losses = totalSeries - totalWins,
                        WinRate = totalSeries > 0 ? Math.Round((double)totalWins / totalSeries * 100, 1) : 0,
                        TopChampions = topChampions
                    });
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(12));
                _cache.Set(cacheKey, allPlayerStats, cacheEntryOptions);
            }

            // Sắp xếp theo vị trí như cũ
            var positionOrder = new List<string> { "Top", "Jungle", "Mid", "ADC", "Support" };
            allPlayerStats = allPlayerStats.OrderBy(p => {
                string pos = p.PlayerInfo?.Position ?? "";
                int index = positionOrder.IndexOf(pos);
                return index == -1 ? 99 : index;
            }).ToList();

            // Nếu bạn muốn phân biệt đội hình hiện tại và cựu thành viên, bạn vẫn có thể dùng danh sách tên hiện tại
            var currentRosterNames = new List<string> { "Doran", "Oner", "Faker", "Peyz", "Keria" };
            ViewBag.CurrentRoster = allPlayerStats.Where(p => currentRosterNames.Contains(p.PlayerInfo?.IngameName, StringComparer.OrdinalIgnoreCase)).ToList();
            ViewBag.FormerPlayers = allPlayerStats.Where(p => !currentRosterNames.Contains(p.PlayerInfo?.IngameName, StringComparer.OrdinalIgnoreCase)).ToList();

            return View();
        }
        // ---------- TRANG DASHBOARD TỪNG TUYỂN THỦ ----------
        public IActionResult Detail(int id)
        {
            var player = _statsContext.Players.FirstOrDefault(p => p.IdPlayer == id);
            if (player == null) return NotFound();

            var t1Team = _statsContext.Teams.FirstOrDefault(t => t.Name == "T1");
            int t1TeamId = t1Team != null ? t1Team.IdTeam : 1;

            var model = new PlayerDashboard
            {
                PlayerInfo = player,
                OpponentTeams = _statsContext.Teams.Where(t => t.IdTeam != 1).Select(t => t.Name).ToList()
            };

            // 1. TÍNH TOÁN WINRATE THEO SERIES (BO3/BO5)
            var allPlayerGames = (from gp in _statsContext.GamePlayers
                                  where gp.PlayerId == id
                                  join gt in _statsContext.GameTeams on gp.GameTeamId equals gt.IdGameTeam
                                  join g in _statsContext.Games on gt.GameId equals g.IdGame
                                  join s in _statsContext.Series on g.SeriesId equals s.IdSeries
                                  join t in _statsContext.Tournaments on s.TournamentId equals t.IdTournament
                                  where gt.TeamId == t1TeamId   // ← thêm điều kiện
                                  select new
                                  {
                                      TournamentName = t.Name,
                                      SeriesId = s.IdSeries,
                                      GameResult = gt.Result
                                  }).ToList();

            // Gom nhóm theo Series trước, rồi gom theo Tournament
            model.TournamentStats = allPlayerGames
                .GroupBy(x => new { x.SeriesId, x.TournamentName })
                .Select(g => new {
                    TournamentName = g.Key.TournamentName,
                    IsSeriesWin = g.Count(x => x.GameResult == "Win") > g.Count(x => x.GameResult == "Loss")
                })
                .GroupBy(x => x.TournamentName)
                .Select(g => {
                    int totalSeries = g.Count();
                    int wins = g.Count(x => x.IsSeriesWin);
                    return new TournamentStat
                    {
                        TournamentName = g.Key,
                        Wins = wins,
                        Losses = totalSeries - wins,
                        WinRate = totalSeries > 0 ? Math.Round((double)wins / totalSeries * 100, 1) : 0
                    };
                }).ToList();

            // ==========================================
            // 2. TÍNH TOÁN THỐNG KÊ THEO CHAMPION (TƯỚNG TỦ)
            // ==========================================

            // BƯỚC 1: Rút dữ liệu thô từ SQL về RAM (Lưu ý chữ .ToList() ở cuối)
            var rawChampData = (from gp in _statsContext.GamePlayers
                                where gp.PlayerId == id
                                join gt in _statsContext.GameTeams on gp.GameTeamId equals gt.IdGameTeam
                                join c in _statsContext.Champions on gp.ChampionId equals c.IdChampion
                                where gt.TeamId == t1TeamId   // ← thêm điều kiện
                                select new
                                {
                                    ChampName = c.Name,
                                    ChampImage = c.ImageUrl,
                                    Result = gt.Result
                                }).ToList();

            // BƯỚC 2: Dùng ngoặc nhọn { } để tính toán dữ liệu đã nằm trên RAM
            model.ChampionStats = rawChampData
                .GroupBy(x => new { x.ChampName, x.ChampImage })
                .Select(g => {
                    int picks = g.Count();
                    int wins = g.Count(x => x.Result == "Win");
                    return new ChampionStat
                    {
                        ChampionName = g.Key.ChampName,
                        ImageUrl = g.Key.ChampImage,
                        Picks = picks,
                        Wins = wins,
                        Losses = picks - wins,
                        WinRate = picks > 0 ? Math.Round((double)wins / picks * 100, 1) : 0,
                        TotalBans = 0 // Sẽ xử lý logic Bans sau
                    };
                })
                .OrderByDescending(x => x.Picks)
                .ToList();

            return View(model);
        }
        // ---------- API LỌC TƯỚNG THEO ĐỐI THỦ (AJAX) ----------
        [HttpGet]
        public IActionResult FilterChampionStats(int playerId, string opponentTeamName)
        {
            // 1. Kết nối qua bảng Series để lấy thông tin Đội đối thủ (TeamOpponentId)
            var t1Team = _statsContext.Teams.FirstOrDefault(t => t.Name == "T1");
            int t1TeamId = t1Team != null ? t1Team.IdTeam : 1;

            var query = from gp in _statsContext.GamePlayers
                        where gp.PlayerId == playerId
                        join gt in _statsContext.GameTeams on gp.GameTeamId equals gt.IdGameTeam
                        join g in _statsContext.Games on gt.GameId equals g.IdGame
                        join s in _statsContext.Series on g.SeriesId equals s.IdSeries
                        join tOpp in _statsContext.Teams on s.TeamOpponentId equals tOpp.IdTeam
                        join c in _statsContext.Champions on gp.ChampionId equals c.IdChampion
                        where gt.TeamId == t1TeamId   // ← thêm điều kiện
                        select new
                        {
                            ChampName = c.Name,
                            ChampImage = c.ImageUrl,
                            Result = gt.Result,
                            OpponentName = tOpp.Name
                        };

            // 2. Nếu Boss có chọn đội để lọc (Khác "ALL") thì áp dụng điều kiện
            if (!string.IsNullOrEmpty(opponentTeamName) && opponentTeamName != "ALL")
            {
                query = query.Where(q => q.OpponentName == opponentTeamName);
            }

            // 3. Rút dữ liệu về RAM
            var rawData = query.ToList();

            // 4. Tính toán trên RAM y như hàm Detail
            var stats = rawData.GroupBy(x => new { x.ChampName, x.ChampImage })
                               .Select(g => {
                                   int picks = g.Count();
                                   int wins = g.Count(x => x.Result != null && x.Result.Trim() == "Win");
                                   return new ChampionStat
                                   {
                                       ChampionName = g.Key.ChampName,
                                       ImageUrl = g.Key.ChampImage,
                                       Picks = picks,
                                       Wins = wins,
                                       Losses = picks - wins,
                                       WinRate = picks > 0 ? Math.Round((double)wins / picks * 100, 1) : 0,
                                       TotalBans = 0 // Tạm để 0, xử lý ở bước sau
                                   };
                               })
                               .OrderByDescending(x => x.Picks)
                               .ToList();

            // Trả về dữ liệu dạng JSON cho Frontend
            return Json(stats);
        }
    }
}