using System;
using System.ComponentModel.DataAnnotations;

namespace T1EsportsWeb.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        public string SenderUsername { get; set; }
        public string ReceiverUsername { get; set; }
        public string MessageContent { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
    }
}