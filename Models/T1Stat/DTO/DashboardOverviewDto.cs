namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class DashboardOverviewDto
    {
        public int DomesticSeriesCount { get; set; }
        public int InternationalSeriesCount { get; set; }
        public int TotalGames { get; set; }
        public int Bo3Count { get; set; }
        public int Bo5Count { get; set; }
        public int Bo1Count { get; set; }
        public int BlueGames { get; set; }
        public int RedGames { get; set; }
        public double BlueWinRate { get; set; }
        public double RedWinRate { get; set; }
    }
}