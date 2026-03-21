namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class PlayerWinrateDto
    {
        public string PlayerName { get; set; }
        public string Role { get; set; }
        public int Games { get; set; }
        public int Wins { get; set; }
        public double WinRate { get; set; }
    }
}
