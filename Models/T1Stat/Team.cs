using System;
using System.Collections.Generic;

namespace T1EsportsWeb.Models.T1Stat;

public partial class Team
{
    public int IdTeam { get; set; }

    public string? Name { get; set; }

    public string? Region { get; set; }

    public string? LogoUrl { get; set; }

    public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();

    public virtual ICollection<GameTeam> GameTeams { get; set; } = new List<GameTeam>();

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();

    public virtual ICollection<Series> SeriesTeamOpponents { get; set; } = new List<Series>();

    public virtual ICollection<Series> SeriesTeamT1s { get; set; } = new List<Series>();
}
