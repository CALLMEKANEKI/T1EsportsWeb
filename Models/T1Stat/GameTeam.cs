using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

[Table("game_teams")]
[Index("Result", Name = "IX_game_teams_result")]
[Index("TeamId", Name = "IX_game_teams_team")]
[Index("GameId", "TeamId", Name = "UQ_game_team", IsUnique = true)]
public partial class GameTeam
{
    [Key]
    [Column("id_game_team")]
    public int IdGameTeam { get; set; }

    [Column("game_id")]
    public int GameId { get; set; }

    [Column("team_id")]
    public int TeamId { get; set; }

    [Column("side")]
    [StringLength(4)]
    public string? Side { get; set; }

    [Column("result")]
    [StringLength(4)]
    public string? Result { get; set; }

    [ForeignKey("GameId")]
    [InverseProperty("GameTeams")]
    public virtual Game Game { get; set; } = null!;

    [InverseProperty("GameTeam")]
    public virtual ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();

    [ForeignKey("TeamId")]
    [InverseProperty("GameTeams")]
    public virtual Team Team { get; set; } = null!;
}
