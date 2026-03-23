namespace T1EsportsWeb.Models.T1Stat.DTO
{
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
}
