using Microsoft.EntityFrameworkCore;
using T1EsportsWeb.Models.T1Stat;
using T1EsportsWeb.Repositories.Interfaces;

namespace T1EsportsWeb.Repositories.Implementations
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(T1StatDbContext context) : base(context)
        {
        }

        public async Task<Team> GetTeamWithDetailsAsync(int id)
        {
            return await _context.Teams
                .Include(t => t.SeriesTeamT1s)     
                .Include(t => t.SeriesTeamOpponents)
                .Include(t => t.GameTeams)
                .FirstOrDefaultAsync(t => t.IdTeam == id);
        }
    }
}