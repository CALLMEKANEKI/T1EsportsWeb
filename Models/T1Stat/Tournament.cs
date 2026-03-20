using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

[Table("tournaments")]
[Index("Name", "Year", Name = "UQ_tournaments_name_year", IsUnique = true)]
public partial class Tournament
{
    [Key]
    [Column("id_tournament")]
    public int IdTournament { get; set; }

    [Column("name")]
    [StringLength(100)]
    public string? Name { get; set; }

    [Column("year")]
    public int? Year { get; set; }

    [Column("region")]
    [StringLength(20)]
    public string? Region { get; set; }

    [Column("isT1winner")]
    [StringLength(10)]
    public string? IsT1winner { get; set; }

    [Column("winner")]
    [StringLength(100)]
    public string? Winner { get; set; }

    [InverseProperty("Tournament")]
    public virtual ICollection<Series> Series { get; set; } = new List<Series>();
}
