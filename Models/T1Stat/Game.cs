using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

[Table("games")]
[Index("DatePlayed", Name = "IX_games_date")]
[Index("Patch", Name = "IX_games_patch")]
[Index("SeriesId", "GameNumber", Name = "UQ_game_in_series", IsUnique = true)]
public partial class Game
{
    [Key]
    [Column("id_game")]
    public int IdGame { get; set; }

    [Column("series_id")]
    public int SeriesId { get; set; }

    [Column("game_number")]
    public int? GameNumber { get; set; }

    [Column("patch")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Patch { get; set; }

    [Column("link")]
    [StringLength(255)]
    public string? Link { get; set; }

    [Column("date_played")]
    public DateOnly? DatePlayed { get; set; }

    [InverseProperty("Game")]
    public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();

    [InverseProperty("Game")]
    public virtual ICollection<GameTeam> GameTeams { get; set; } = new List<GameTeam>();

    [ForeignKey("SeriesId")]
    [InverseProperty("Games")]
    public virtual Series Series { get; set; } = null!;
}
