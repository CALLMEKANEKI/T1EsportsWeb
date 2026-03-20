using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

[Table("series")]
[Index("TournamentId", "TeamOpponentId", "MatchDate", Name = "UQ_series", IsUnique = true)]
public partial class Series
{
    [Key]
    [Column("id_series")]
    public int IdSeries { get; set; }

    [Column("tournament_id")]
    public int TournamentId { get; set; }

    [Column("team_t1_id")]
    public int TeamT1Id { get; set; }

    [Column("team_opponent_id")]
    public int TeamOpponentId { get; set; }

    [Column("match_date")]
    public DateOnly? MatchDate { get; set; }

    [Column("best_of")]
    public int? BestOf { get; set; }

    [InverseProperty("Series")]
    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    [ForeignKey("TeamOpponentId")]
    [InverseProperty("SeriesTeamOpponents")]
    public virtual Team TeamOpponent { get; set; } = null!;

    [ForeignKey("TeamT1Id")]
    [InverseProperty("SeriesTeamT1s")]
    public virtual Team TeamT1 { get; set; } = null!;

    [ForeignKey("TournamentId")]
    [InverseProperty("Series")]
    public virtual Tournament Tournament { get; set; } = null!;
}
