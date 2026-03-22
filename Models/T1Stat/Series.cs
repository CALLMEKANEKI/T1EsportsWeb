using System;
using System.Collections.Generic;

namespace T1EsportsWeb.Models.T1Stat;

public partial class Series
{
    public int IdSeries { get; set; }

    public int TournamentId { get; set; }

    public int TeamT1Id { get; set; }

    public int TeamOpponentId { get; set; }

    public DateOnly? MatchDate { get; set; }

    public int? BestOf { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    public virtual Team TeamOpponent { get; set; } = null!;

    public virtual Team TeamT1 { get; set; } = null!;

    public virtual Tournament Tournament { get; set; } = null!;
}
