namespace T1EsportsWeb.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string IngameName { get; set; }
        public string Role { get; set; } // Top, Jungle, Mid, ADC, Support
        public string ImageUrl { get; set; } // Link ảnh siêu ngầu
        public string SignatureChampion { get; set; } // Tướng tủ
    }
}
