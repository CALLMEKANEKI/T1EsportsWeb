using System;
using System.Collections.Generic;

namespace T1EsportsWeb.Models.T1Stat;

public partial class GameTeam
{
    public int IdGameTeam { get; set; }

    public int GameId { get; set; }

    public int TeamId { get; set; }

    public string? Side { get; set; }

    public string? Result { get; set; }

    public virtual Game Game { get; set; } = null!;

    public virtual ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();

    public virtual Team Team { get; set; } = null!;
}
