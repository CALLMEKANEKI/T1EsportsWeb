using System;
using System.ComponentModel.DataAnnotations;

namespace T1EsportsWeb.Models
{
    public class PickEmPrediction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } // Ai là người dự đoán?

        [Required]
        public int SeriesId { get; set; } // Dự đoán cho trận nào bên T1Stat?

        [Required]
        public string PredictedScore { get; set; } // VD: "2-0", "1-2"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsProcessed { get; set; } = false; // Dùng để sau này check xem admin đã phát thưởng cho trận này chưa
    }
}