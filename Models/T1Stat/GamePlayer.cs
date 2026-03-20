using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

[Table("game_players")]
[Index("ChampionId", Name = "IX_game_players_champion")]
[Index("PlayerId", Name = "IX_game_players_player")]
[Index("GameTeamId", "PlayerId", Name = "UQ_game_team_player", IsUnique = true)]
public partial class GamePlayer
{
    [Key]
    [Column("id_game_player")]
    public int IdGamePlayer { get; set; }

    [Column("game_team_id")]
    public int GameTeamId { get; set; }

    [Column("player_id")]
    public int PlayerId { get; set; }

    [Column("champion_id")]
    public int ChampionId { get; set; }

    [Column("pick_order")]
    public int? PickOrder { get; set; }

    [ForeignKey("ChampionId")]
    [InverseProperty("GamePlayers")]
    public virtual Champion Champion { get; set; } = null!;

    [ForeignKey("GameTeamId")]
    [InverseProperty("GamePlayers")]
    public virtual GameTeam GameTeam { get; set; } = null!;

    [ForeignKey("PlayerId")]
    [InverseProperty("GamePlayers")]
    public virtual Player Player { get; set; } = null!;
}
