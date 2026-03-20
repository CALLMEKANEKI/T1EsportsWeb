using T1EsportsWeb.Models.T1Stat;

namespace T1EsportsWeb.Repositories.Interfaces
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        // Các phương thức đặc thù cho Team (nếu có)
        Task<Team> GetTeamWithDetailsAsync(int id); // Ví dụ: lấy team kèm series, games
    }
}
