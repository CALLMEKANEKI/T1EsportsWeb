using System;
using System.Collections.Generic;

namespace T1EsportsWeb.Models.T1Stat;

public partial class Game
{
    public int IdGame { get; set; }

    public int SeriesId { get; set; }

    public int? GameNumber { get; set; }

    public string? Patch { get; set; }

    public string? Link { get; set; }

    public DateOnly? DatePlayed { get; set; }

    public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();

    public virtual ICollection<GameTeam> GameTeams { get; set; } = new List<GameTeam>();

    public virtual Series Series { get; set; } = null!;
}
