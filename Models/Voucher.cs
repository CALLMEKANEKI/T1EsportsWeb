using System.ComponentModel.DataAnnotations;

namespace T1EsportsWeb.Models
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EventCode { get; set; } // Mã code để khách nhập

        public string Title { get; set; } // Tên hiển thị trên thẻ quà (VD: Voucher Tân Thủ)

        public string Description { get; set; } // Mô tả công dụng

        public int DiscountPercent { get; set; } // % Giảm giá

        public int Quantity { get; set; } // Số lượng tồn

        public int CostPoints { get; set; } // Số điểm cần để đổi (Đổi tên từ PointsRequired)
    }
}