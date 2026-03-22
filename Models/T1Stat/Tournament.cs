using System;
using System.Collections.Generic;

namespace T1EsportsWeb.Models.T1Stat;

public partial class Tournament
{
    public int IdTournament { get; set; }

    public string? Name { get; set; }

    public int? Year { get; set; }

    public string? Region { get; set; }

    public string? IsT1winner { get; set; }

    public string? Winner { get; set; }

    public virtual ICollection<Series> Series { get; set; } = new List<Series>();
}
