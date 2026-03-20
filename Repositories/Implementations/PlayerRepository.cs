using Microsoft.EntityFrameworkCore;
using T1EsportsWeb.Models.T1Stat;
using T1EsportsWeb.Repositories.Interfaces;

namespace T1EsportsWeb.Repositories.Implementations
{
    public class PlayerRepository : GenericRepository<Player>, IPlayerRepository
    {
        public PlayerRepository(T1StatDbContext context) : base(context)
        {
        }

        // Lấy đội hình hiện tại (ví dụ dựa trên thời gian hoặc cột IsActive - nếu có)
        public async Task<IEnumerable<Player>> GetCurrentRosterAsync()
        {
            // Nếu có cột IsActive, dùng: .Where(p => p.IsActive == true)
            // Nếu không, tạm thời trả về tất cả (bạn cần thêm cột hoặc logic khác)
            return await _context.Players.ToListAsync();
        }

        public async Task<IEnumerable<Player>> GetFormerPlayersAsync()
        {
            // Tương tự, nếu có IsActive thì .Where(p => p.IsActive == false)
            return await _context.Players.ToListAsync(); // Tạm thời
        }
    }
}