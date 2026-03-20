using T1EsportsWeb.Models.T1Stat;

namespace T1EsportsWeb.Repositories.Interfaces
{
    public interface IPlayerRepository : IGenericRepository<Player>
    {
        Task<IEnumerable<Player>> GetCurrentRosterAsync(); // Lọc đội hình hiện tại
        Task<IEnumerable<Player>> GetFormerPlayersAsync(); // Cựu thành viên
    }
}