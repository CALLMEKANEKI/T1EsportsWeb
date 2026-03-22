using System;
using System.Collections.Generic;

namespace T1EsportsWeb.Models.T1Stat;

public partial class Player
{
    public int IdPlayer { get; set; }

    public string? IngameName { get; set; }

    public string? FullName { get; set; }

    public string? Position { get; set; }

    public string? PhotoUrl { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? Country { get; set; }

    public int? TeamId { get; set; }

    public virtual ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();

    public virtual Team? Team { get; set; }

}
