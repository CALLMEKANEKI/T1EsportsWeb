using System.ComponentModel.DataAnnotations;

namespace T1EsportsWeb.Models
{
    public class UserVoucher
    {
        [Key]
        public int Id { get; set; }

        // Đổi thành int? (thêm dấu ?) để tránh lỗi ràng buộc khóa ngoại khi lưu DB
        public int? UserId { get; set; }
        public int? VoucherId { get; set; }

        // --- BỔ SUNG 3 TRƯỜNG NÀY ĐỂ FIX LỖI CONTROLLER VÀ CHỨA MÃ RANDOM ---
        public string Username { get; set; }
        public string VoucherCode { get; set; }
        public int DiscountPercent { get; set; }

        public bool IsUsed { get; set; } = false;
        public DateTime ExpirationDate { get; set; }
    }
}