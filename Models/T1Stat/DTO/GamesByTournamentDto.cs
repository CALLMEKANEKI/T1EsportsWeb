namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class GamesByTournamentDto
    {
        public int IdTournament { get; set; } 
        public string TournamentName { get; set; }
        public int Year { get; set; }
        public string Region { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public double WinRate { get; set; }

        public int GamesKRWon { get; set; }
        public int GamesKRLost { get; set; }
        public int GamesINTWon { get; set; }
        public int GamesINTLost { get; set; }
    }
}