using System;
using System.Collections.Generic;

namespace T1EsportsWeb.Models.T1Stat;

public partial class GamePlayer
{
    public int IdGamePlayer { get; set; }

    public int GameTeamId { get; set; }

    public int PlayerId { get; set; }

    public int ChampionId { get; set; }

    public int? PickOrder { get; set; }

    public virtual Champion Champion { get; set; } = null!;

    public virtual GameTeam GameTeam { get; set; } = null!;

    public virtual Player Player { get; set; } = null!;
}
