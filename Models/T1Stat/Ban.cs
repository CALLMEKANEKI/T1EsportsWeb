using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

[Table("bans")]
[Index("ChampionId", Name = "IX_bans_champion")]
[Index("TeamId", Name = "IX_bans_team")]
public partial class Ban
{
    [Key]
    [Column("id_ban")]
    public int IdBan { get; set; }

    [Column("game_id")]
    public int GameId { get; set; }

    [Column("team_id")]
    public int TeamId { get; set; }

    [Column("champion_id")]
    public int ChampionId { get; set; }

    [Column("ban_order")]
    public int? BanOrder { get; set; }

    [ForeignKey("ChampionId")]
    [InverseProperty("Bans")]
    public virtual Champion Champion { get; set; } = null!;

    [ForeignKey("GameId")]
    [InverseProperty("Bans")]
    public virtual Game Game { get; set; } = null!;

    [ForeignKey("TeamId")]
    [InverseProperty("Bans")]
    public virtual Team Team { get; set; } = null!;
}
