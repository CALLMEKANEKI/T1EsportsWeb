using System.ComponentModel.DataAnnotations;

namespace T1EsportsWeb.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; } 
        public string? Username { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; }

        public decimal Total => Price * Quantity;
    }
}