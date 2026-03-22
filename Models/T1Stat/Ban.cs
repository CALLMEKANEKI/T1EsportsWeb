using System;
using System.Collections.Generic;

namespace T1EsportsWeb.Models.T1Stat;

public partial class Ban
{
    public int IdBan { get; set; }

    public int GameId { get; set; }

    public int TeamId { get; set; }

    public int ChampionId { get; set; }

    public int? BanOrder { get; set; }

    public virtual Champion Champion { get; set; } = null!;

    public virtual Game Game { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}
