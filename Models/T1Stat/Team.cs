using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

[Table("teams")]
[Index("Name", Name = "UQ__teams__72E12F1B241F3630", IsUnique = true)]
public partial class Team
{
    [Key]
    [Column("id_team")]
    public int IdTeam { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string? Name { get; set; }

    [Column("region")]
    [StringLength(20)]
    public string? Region { get; set; }

    [Column("logo_url")]
    [StringLength(255)]
    public string? LogoUrl { get; set; }

    [InverseProperty("Team")]
    public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();

    [InverseProperty("Team")]
    public virtual ICollection<GameTeam> GameTeams { get; set; } = new List<GameTeam>();

    [InverseProperty("TeamOpponent")]
    public virtual ICollection<Series> SeriesTeamOpponents { get; set; } = new List<Series>();

    [InverseProperty("TeamT1")]
    public virtual ICollection<Series> SeriesTeamT1s { get; set; } = new List<Series>();
    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
}