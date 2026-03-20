using Microsoft.EntityFrameworkCore;
using T1EsportsWeb.Models.T1Stat;
using T1EsportsWeb.Repositories.Interfaces;

namespace T1EsportsWeb.Repositories.Implementations
{
    public class TournamentRepository : GenericRepository<Tournament>, ITournamentRepository
    {
        public TournamentRepository(T1StatDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tournament>> GetTournamentsByYearAsync(int year)
        {
            return await _context.Tournaments
                .Where(t => t.Year == year)
                .ToListAsync();
        }
    }
}