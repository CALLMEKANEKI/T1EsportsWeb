using System;
using System.ComponentModel.DataAnnotations;

namespace T1EsportsWeb.Models
{
    public class PickEmMatch
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TournamentName { get; set; } // VD: "LCK Mùa Xuân 2026"

        [Required]
        public DateTime MatchTime { get; set; } // Thời gian đá để đếm ngược/khóa dự đoán

        [Required]
        public string OpponentName { get; set; } // Tên đối thủ (VD: Gen.G, HLE...)

        public bool IsLocked { get; set; } = false; // Bật true thì user không được sửa/chọn nữa

        public string? ActualScore { get; set; } // Kết quả thật (Admin sẽ điền sau khi đá xong)

        public bool IsRewarded { get; set; } = false; // Đã phát thưởng cho trận này chưa?
    }
}