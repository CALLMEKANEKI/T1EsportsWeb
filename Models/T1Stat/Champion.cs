using System;
using System.Collections.Generic;

namespace T1EsportsWeb.Models.T1Stat;

public partial class Champion
{
    public int IdChampion { get; set; }

    public string? Name { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();

    public virtual ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
}
