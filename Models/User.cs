using System.ComponentModel.DataAnnotations;

namespace T1EsportsWeb.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }

        public string Email { get; set; }
        public int T1Points { get; set; } = 0; // Điểm cày từ game

        public string Role { get; set; } = "User";
    }
}