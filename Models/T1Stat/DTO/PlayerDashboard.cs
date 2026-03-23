using T1EsportsWeb.Controllers;

namespace T1EsportsWeb.Models.T1Stat.DTO
{
    public class PlayerDashboard
    {
        public Player PlayerInfo { get; set; }
        public List<TournamentStat> TournamentStats { get; set; } = new List<TournamentStat>();
        public List<ChampionStat> ChampionStats { get; set; } = new List<ChampionStat>();
        public List<string> OpponentTeams { get; set; } = new List<string>();
    }
}
