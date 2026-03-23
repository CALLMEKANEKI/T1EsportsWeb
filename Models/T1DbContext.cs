using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models
{
    public class T1DbContext : DbContext
    {
        public T1DbContext(DbContextOptions<T1DbContext> options) : base(options)
        {
        }

        // Báo cho EF Core biết hãy tạo một bảng tên là 'Players' dựa trên class 'Player'
        public DbSet<Player> Players { get; set; }
        // Khai báo 4 bảng để nó tạo trong SQL Server
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<PickEmPrediction> PickEmPredictions { get; set; }
        public DbSet<PickEmMatch> PickEmMatches { get; set; }
    }
}