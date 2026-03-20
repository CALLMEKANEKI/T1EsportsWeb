namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class HeadToHeadDto
    {
        public string OpponentName { get; set; }
        public int DomesticGames { get; set; }
        public int DomesticWins { get; set; }
        public double DomesticWinRate { get; set; }
        public int InternationalGames { get; set; }
        public int InternationalWins { get; set; }
        public double InternationalWinRate { get; set; }
    }
}
