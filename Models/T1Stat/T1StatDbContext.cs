using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

public partial class T1StatDbContext : DbContext
{
    public T1StatDbContext()
    {
    }

    public T1StatDbContext(DbContextOptions<T1StatDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ban> Bans { get; set; }

    public virtual DbSet<Champion> Champions { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<GamePlayer> GamePlayers { get; set; }

    public virtual DbSet<GameTeam> GameTeams { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Series> Series { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<Tournament> Tournaments { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=Anhgiangdeptrai;Database=T1_Stats;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.IdBan).HasName("PK__bans__D506889121A2F293");

            entity.HasOne(d => d.Champion).WithMany(p => p.Bans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bans_champion");

            entity.HasOne(d => d.Game).WithMany(p => p.Bans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bans_game");

            entity.HasOne(d => d.Team).WithMany(p => p.Bans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bans_team");
        });

        modelBuilder.Entity<Champion>(entity =>
        {
            entity.HasKey(e => e.IdChampion).HasName("PK__champion__000A3023BD0BB9AD");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.IdGame).HasName("PK__games__0E819B2CC1889B03");

            entity.HasOne(d => d.Series).WithMany(p => p.Games)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_games_series");
        });

        modelBuilder.Entity<GamePlayer>(entity =>
        {
            entity.HasKey(e => e.IdGamePlayer).HasName("PK__game_pla__563827FB5560D87E");

            entity.HasOne(d => d.Champion).WithMany(p => p.GamePlayers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_game_players_champion");

            entity.HasOne(d => d.GameTeam).WithMany(p => p.GamePlayers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_game_players_game_team");

            entity.HasOne(d => d.Player).WithMany(p => p.GamePlayers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_game_players_player");
        });

        modelBuilder.Entity<GameTeam>(entity =>
        {
            entity.HasKey(e => e.IdGameTeam).HasName("PK__game_tea__C82B2E8A2BE2EC6A");

            entity.HasOne(d => d.Game).WithMany(p => p.GameTeams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_game_teams_game");

            entity.HasOne(d => d.Team).WithMany(p => p.GameTeams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_game_teams_team");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.IdPlayer).HasName("PK__players__45CF72B10FAB3EF2");

            entity.HasOne(d => d.Team).WithMany(p => p.Players).HasConstraintName("FK_players_teams");
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasKey(e => e.IdSeries).HasName("PK__series__F4DBDFD5D494A64E");

            entity.Property(e => e.BestOf).HasDefaultValue(3);

            entity.HasOne(d => d.TeamOpponent).WithMany(p => p.SeriesTeamOpponents)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_series_team_opponent");

            entity.HasOne(d => d.TeamT1).WithMany(p => p.SeriesTeamT1s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_series_team_t1");

            entity.HasOne(d => d.Tournament).WithMany(p => p.Series)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_series_tournament");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.IdTeam).HasName("PK__teams__C6D204E794555A32");
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasKey(e => e.IdTournament).HasName("PK__tourname__1471F11D3A1596D7");

            entity.Property(e => e.Region).HasDefaultValue("KR");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
