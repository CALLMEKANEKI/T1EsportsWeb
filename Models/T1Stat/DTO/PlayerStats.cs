using T1EsportsWeb.Controllers;

namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class PlayerStats
    {
        public Player PlayerInfo { get; set; }
        public int TotalGames { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }
        public List<TopChampion> TopChampions { get; set; } = new List<TopChampion>();
    }
}
