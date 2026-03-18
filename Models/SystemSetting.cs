using System.ComponentModel.DataAnnotations;

namespace T1EsportsWeb.Models
{
    public class SystemSetting
    {
        [Key]
        public string Key { get; set; } // Tên cài đặt (Ví dụ: "ShippingFee")
        public string Value { get; set; } // Giá trị (Ví dụ: "30000")
        public string Description { get; set; } // Mô tả cho Admin dễ hiểu
    }
}