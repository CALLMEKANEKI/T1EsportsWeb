namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class GameDto
    {
        public int GameId { get; set; }  // Thêm GameId
        public int GameNumber { get; set; }
        public string Patch { get; set; }
        public string Result { get; set; }
        public string Side { get; set; }
        public string Link { get; set; }
    }
}
