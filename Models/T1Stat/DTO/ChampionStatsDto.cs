namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class ChampionStatsDto
    {
        public string ChampionName { get; set; }
        public int Picks { get; set; }
        public int Bans { get; set; }
        public int Wins { get; set; }
        public double WinRate { get; set; }
    }
}
