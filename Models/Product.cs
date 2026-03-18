using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T1EsportsWeb.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Column(TypeName = "decimal(18, 0)")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        public string Description { get; set; }

        // --- 2 CỘT MỚI THÊM ĐỂ QUẢN LÝ KHO ---

        // Số lượng sản phẩm còn trong kho
        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public int StockQuantity { get; set; } = 0;

        // Kích cỡ (Tạm thời nhập tay ví dụ: "S, M, L, XL")
        public string Sizes { get; set; }
    }
}