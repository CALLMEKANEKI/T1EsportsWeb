using System;
using System.ComponentModel.DataAnnotations;

namespace T1EsportsWeb.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public string Username { get; set; } // Liên kết với tài khoản người mua
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Chờ xử lý"; // Các trạng thái: Chờ xử lý, Đang giao, Đã hoàn thành
    }
}