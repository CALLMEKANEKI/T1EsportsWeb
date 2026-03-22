namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class SeriesByTournamentDto
    {
        public int IdTournament { get; set; }
        public string TournamentName { get; set; }
        public int Year { get; set; }
        public string Region { get; set; }
        public int SeriesWon { get; set; }      // Tổng số series thắng
        public int SeriesLost { get; set; }     // Tổng số series thua
        public double WinRate { get; set; }
        public bool IsT1Winner { get; set; }

        // Bổ sung các trường riêng cho KR và INT
        public int KRWon { get; set; }
        public int KRLost { get; set; }
        public int INTWon { get; set; }
        public int INTLost { get; set; }
    }
}