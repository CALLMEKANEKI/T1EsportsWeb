using Microsoft.EntityFrameworkCore;
using T1EsportsWeb.Models.T1Stat;
using T1EsportsWeb.Repositories.Interfaces;

namespace T1EsportsWeb.Repositories.Implementations
{
    public class ChampionRepository : GenericRepository<Champion>, IChampionRepository
    {
        public ChampionRepository(T1StatDbContext context) : base(context)
        {
        }

        public async Task<Champion> GetChampionWithPicksAsync(int id)
        {
            return await _context.Champions
                .Include(c => c.GamePlayers)
                .FirstOrDefaultAsync(c => c.IdChampion == id);
        }
    }
}