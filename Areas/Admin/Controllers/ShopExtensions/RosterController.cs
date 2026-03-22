using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using T1EsportsWeb.Models;
using T1EsportsWeb.Models.T1Stat;

namespace T1EsportsWeb.Controllers
{
    // CÁC CLASS ĐƯỢC CHỨA Ở ĐÂY LÀ AN TOÀN TUYỆT ĐỐI
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

    public class RosterController : Controller
    {
        private readonly T1StatDbContext _statsContext;
        private readonly IMemoryCache _cache;

        public RosterController(T1StatDbContext statsContext, IMemoryCache cache)
        {
            _statsContext = statsContext;
            _cache = cache;
        }

        public IActionResult Index()
        {
            const string cacheKey = "T1RosterStatsData_Top18"; // Đổi key cache để làm mới dữ liệu

            if (!_cache.TryGetValue(cacheKey, out List<PlayerStatsViewModel>? allPlayerStats) || allPlayerStats == null)
            {
                allPlayerStats = new List<PlayerStatsViewModel>();

                var t1Team = _statsContext.Teams.FirstOrDefault(t => t.Name == "T1");
                int t1TeamId = t1Team != null ? t1Team.IdTeam : 1;

                // 🎯 THAY ĐỔI TẠI ĐÂY: Chỉ lấy 18 người đầu tiên dựa trên ID
                var t1Players = _statsContext.Players
                                             .Where(p => p.TeamId == t1TeamId)
                                             .OrderBy(p => p.IdPlayer) // Sắp xếp theo ID tăng dần
                                             .Take(18)                 // Chỉ lấy đúng 18 dòng đầu
                                             .ToList();

                var allChampions = _statsContext.Champions.ToList();

                foreach (var player in t1Players)
                {
                    // Lấy dữ liệu trận đấu (Giữ nguyên logic cũ đã tối ưu Join)
                    var playerMatchData = (from gp in _statsContext.GamePlayers
                                           join gt in _statsContext.GameTeams on gp.GameTeamId equals gt.IdGameTeam
                                           where gp.PlayerId == player.IdPlayer
                                           select new
                                           {
                                               ChampionId = gp.ChampionId,
                                               Result = gt.Result
                                           }).ToList();

                    int totalGames = playerMatchData.Count;
                    int totalWins = playerMatchData.Count(m => m.Result == "Win");

                    var championCounts = playerMatchData.GroupBy(g => g.ChampionId)
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

                    allPlayerStats.Add(new PlayerStatsViewModel
                    {
                        PlayerInfo = player,
                        TotalGames = totalGames,
                        Wins = totalWins,
                        Losses = totalGames - totalWins,
                        WinRate = totalGames > 0 ? Math.Round((double)totalWins / totalGames * 100, 1) : 0,
                        TopChampions = topChampions
                    });
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(12));
                _cache.Set(cacheKey, allPlayerStats, cacheEntryOptions);
            }

            // Phần phân loại Current Roster và Former Players giữ nguyên như cũ...
            var currentRosterNames = new List<string> { "Doran", "Oner", "Faker", "Peyz", "Keria" };

            // Sắp xếp thứ tự vị trí
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
    }
}