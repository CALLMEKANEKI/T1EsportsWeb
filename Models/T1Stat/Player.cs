using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

[Table("players")]
[Index("IngameName", Name = "UQ__players__2AF4ED2AA199B9C3", IsUnique = true)]
public partial class Player
{
    [Key]
    [Column("id_player")]
    public int IdPlayer { get; set; }

    [Column("ingame_name")]
    [StringLength(50)]
    public string? IngameName { get; set; }

    [Column("full_name")]
    [StringLength(100)]
    public string? FullName { get; set; }

    [Column("position")]
    [StringLength(10)]
    public string? Position { get; set; }

    [Column("photo_url")]
    [StringLength(255)]
    public string? PhotoUrl { get; set; }

    [Column("birth_date")]
    public DateOnly? BirthDate { get; set; }

    [Column("country")]
    [StringLength(50)]
    public string? Country { get; set; }

    [InverseProperty("Player")]
    public virtual ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
}
