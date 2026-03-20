using T1EsportsWeb.Models.T1Stat;

namespace T1EsportsWeb.Repositories.Interfaces
{
    public interface ITournamentRepository : IGenericRepository<Tournament>
    {
        Task<IEnumerable<Tournament>> GetTournamentsByYearAsync(int year);
    }
}