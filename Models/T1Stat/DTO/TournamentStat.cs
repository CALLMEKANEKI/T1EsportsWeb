namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class TournamentStat
    {
        public string? TournamentName { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinRate { get; set; }
    }
}
