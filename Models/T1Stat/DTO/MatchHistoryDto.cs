namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class MatchHistoryDto
    {
        public int SeriesId { get; set; }
        public DateTime MatchDate { get; set; }
        public string OpponentName { get; set; }
        public string Result { get; set; }        // "Win" / "Loss" / "Draw"
        public int? BestOf { get; set; }
        public string TournamentName { get; set; }
        public int TournamentYear { get; set; }
        public List<GameDto> Games { get; set; }
    }
}
