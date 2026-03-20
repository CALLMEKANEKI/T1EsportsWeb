using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

[Table("champions")]
[Index("Name", Name = "UQ__champion__72E12F1B5FA591F3", IsUnique = true)]
public partial class Champion
{
    [Key]
    [Column("id_champion")]
    public int IdChampion { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string? Name { get; set; }

    [Column("image_url")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [InverseProperty("Champion")]
    public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();

    [InverseProperty("Champion")]
    public virtual ICollection<GamePlayer> GamePlayers { get; set; } = new List<GamePlayer>();
}
