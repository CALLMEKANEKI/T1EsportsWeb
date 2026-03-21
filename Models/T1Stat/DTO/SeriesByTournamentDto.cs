namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class SeriesByTournamentDto
    {
        public string TournamentName { get; set; }
        public int Year { get; set; }
        public string Region { get; set; } // "KR" hoặc "INT"
        public int SeriesWon { get; set; }
        public int SeriesLost { get; set; }
        public bool IsT1Winner { get; set; } // Tournament có T1 vô địch?
    }
}