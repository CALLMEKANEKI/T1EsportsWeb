using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using T1EsportsWeb.Models;
using T1EsportsWeb.Models.T1Stat;

namespace T1EsportsWeb.Controllers
{
    // ==========================================
    // CÁC VIEWMODEL CHO TRANG INDEX
    // ==========================================
    public class PlayerStatsViewModel
    {
        public T1EsportsWeb.Models.T1Stat.Player? PlayerInfo { get; set; }
        public int TotalGames { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }
        public List<TopChampionViewModel> TopChampions { get; set; } = new List<TopChampionViewModel>();
    }

    public class TopChampionViewModel
    {
        public string? Name { get; set; }
        public string? Image { get; set; }
        public int Count { get; set; }
    }

    // ==========================================
    // CÁC VIEWMODEL MỚI CHO TRANG DASHBOARD (DETAIL)
    // ==========================================
    public class PlayerDashboardViewModel
    {
        public T1EsportsWeb.Models.T1Stat.Player? PlayerInfo { get; set; }
        public List<TournamentStat> TournamentStats { get; set; } = new List<TournamentStat>();
        public List<ChampionStat> ChampionStats { get; set; } = new List<ChampionStat>();
        public List<string> OpponentTeams { get; set; } = new List<string>();
    }

    public class TournamentStat
    {
        public string? TournamentName { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }
    }

    public class ChampionStat
    {
        public string? ChampionName { get; set; }
        public string? ImageUrl { get; set; }
        public int Picks { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }
        public int TotalBans { get; set; }
    }

    // ==========================================
    // CONTROLLER CHÍNH
    // ==========================================
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

            // Nếu Boss đang test, tạm thời tắt Cache bằng cách đổi tên key để nó luôn load mới:
            // const string cacheKey = "T1RosterStatsData_Test_01"; 

            if (!_cache.TryGetValue(cacheKey, out List<PlayerStatsViewModel>? allPlayerStats) || allPlayerStats == null)
            {
                allPlayerStats = new List<PlayerStatsViewModel>();

                var t1Team = _statsContext.Teams.FirstOrDefault(t => t.Name == "T1");
                int t1TeamId = t1Team != null ? t1Team.IdTeam : 1;

                var t1Players = _statsContext.Players
                                             .Where(p => p.TeamId == t1TeamId)
                                             .OrderBy(p => p.IdPlayer)
                                             .Take(18)
                                             .ToList();

                var allChampions = _statsContext.Champions.ToList();

                foreach (var player in t1Players)
                {
                    // 1. RÚT DỮ LIỆU THÔ VỀ RAM (Lấy thêm SeriesId từ bảng Games)
                    var playerGames = (from gp in _statsContext.GamePlayers
                                       join gt in _statsContext.GameTeams on gp.GameTeamId equals gt.IdGameTeam
                                       join g in _statsContext.Games on gt.GameId equals g.IdGame
                                       where gp.PlayerId == player.IdPlayer
                                       select new
                                       {
                                           SeriesId = g.SeriesId,
                                           ChampionId = gp.ChampionId,
                                           Result = gt.Result
                                       }).ToList();

                    // 2. TÍNH TOÁN THEO SERIES (BO3/BO5)
                    var seriesStats = playerGames.GroupBy(x => x.SeriesId)
                                                 .Select(g => new {
                                                     // Nếu số ván thắng > ván thua -> Thắng cả Series
                                                     IsWin = g.Count(x => x.Result != null && x.Result.Trim() == "Win") >
                                                             g.Count(x => x.Result != null && x.Result.Trim() == "Loss")
                                                 }).ToList();

                    int totalSeries = seriesStats.Count;
                    int totalWins = seriesStats.Count(s => s.IsWin);

                    // 3. TÍNH TƯỚNG TỦ (Tướng thì vẫn phải đếm theo Game vì mỗi ván chọn 1 tướng)
                    var championCounts = playerGames.GroupBy(g => g.ChampionId)
                                                    .OrderByDescending(g => g.Count())
                                                    .Take(3)
                                                    .Select(g => new { ChampId = g.Key, PlayCount = g.Count() })
                                                    .ToList();

                    var topChampions = new List<TopChampionViewModel>();
                    foreach (var top in championCounts)
                    {
                        var champInfo = allChampions.FirstOrDefault(c => c.IdChampion == top.ChampId);
                        if (champInfo != null)
                        {
                            topChampions.Add(new TopChampionViewModel
                            {
                                Name = champInfo.Name,
                                Image = champInfo.ImageUrl,
                                Count = top.PlayCount
                            });
                        }
                    }

                    // 4. GÁN DỮ LIỆU SERIES VÀO THẺ BÀI
                    allPlayerStats.Add(new PlayerStatsViewModel
                    {
                        PlayerInfo = player,
                        TotalGames = totalSeries, // HIỂN THỊ SỐ SERIES
                        Wins = totalWins,         // HIỂN THỊ SỐ SERIES THẮNG
                        Losses = totalSeries - totalWins,
                        WinRate = totalSeries > 0 ? Math.Round((double)totalWins / totalSeries * 100, 1) : 0,
                        TopChampions = topChampions
                    });
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(12));
                _cache.Set(cacheKey, allPlayerStats, cacheEntryOptions);
            }

            var currentRosterNames = new List<string> { "Doran", "Oner", "Faker", "Peyz", "Keria" };
            var positionOrder = new List<string> { "Top", "Jungle", "Mid", "ADC", "Support" };

            allPlayerStats = allPlayerStats.OrderBy(p => {
                string pos = p.PlayerInfo?.Position ?? "";
                int index = positionOrder.IndexOf(pos);
                return index == -1 ? 99 : index;
            }).ToList();

            ViewBag.CurrentRoster = allPlayerStats.Where(p => currentRosterNames.Contains(p.PlayerInfo?.IngameName, StringComparer.OrdinalIgnoreCase)).ToList();
            ViewBag.FormerPlayers = allPlayerStats.Where(p => !currentRosterNames.Contains(p.PlayerInfo?.IngameName, StringComparer.OrdinalIgnoreCase)).ToList();

            return View();
        }

        // ---------- TRANG DASHBOARD TỪNG TUYỂN THỦ ----------
        public IActionResult Detail(int id)
        {
            var player = _statsContext.Players.FirstOrDefault(p => p.IdPlayer == id);
            if (player == null) return NotFound();

            var model = new PlayerDashboardViewModel
            {
                PlayerInfo = player,
                OpponentTeams = _statsContext.Teams.Where(t => t.IdTeam != 1).Select(t => t.Name).ToList() // Lấy các đội không phải T1 để làm bộ lọc
            };

            // 1. TÍNH TOÁN WINRATE THEO SERIES (BO3/BO5)
            var allPlayerGames = (from gp in _statsContext.GamePlayers
                                  where gp.PlayerId == id
                                  join gt in _statsContext.GameTeams on gp.GameTeamId equals gt.IdGameTeam
                                  join g in _statsContext.Games on gt.GameId equals g.IdGame
                                  join s in _statsContext.Series on g.SeriesId equals s.IdSeries
                                  join t in _statsContext.Tournaments on s.TournamentId equals t.IdTournament
                                  select new
                                  {
                                      TournamentName = t.Name,
                                      SeriesId = s.IdSeries,
                                      GameResult = gt.Result // Giả sử là "Win" hoặc "Loss"
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
                                select new
                                {
                                    ChampName = c.Name,
                                    ChampImage = c.ImageUrl,
                                    Result = gt.Result
                                }).ToList(); // <-- CHÌA KHÓA NẰM Ở ĐÂY

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
            var query = from gp in _statsContext.GamePlayers
                        where gp.PlayerId == playerId
                        join gt in _statsContext.GameTeams on gp.GameTeamId equals gt.IdGameTeam
                        join g in _statsContext.Games on gt.GameId equals g.IdGame
                        join s in _statsContext.Series on g.SeriesId equals s.IdSeries
                        join tOpp in _statsContext.Teams on s.TeamOpponentId equals tOpp.IdTeam
                        join c in _statsContext.Champions on gp.ChampionId equals c.IdChampion
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