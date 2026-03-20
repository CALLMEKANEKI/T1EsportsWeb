using T1EsportsWeb.Models.T1Stat;

namespace T1EsportsWeb.Repositories.Interfaces
{
    public interface IChampionRepository : IGenericRepository<Champion>
    {
        Task<Champion> GetChampionWithPicksAsync(int id);
    }
}