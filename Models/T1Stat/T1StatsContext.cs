using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace T1EsportsWeb.Models.T1Stat;

public partial class T1StatsContext : DbContext
{
    public T1StatsContext()
    {
    }

    public T1StatsContext(DbContextOptions<T1StatsContext> options)
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=PhiAnh\\MSSQLSERVER03;Database=T1_Stats;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.IdBan).HasName("PK__bans__D5068891AD209F8B");

            entity.ToTable("bans");

            entity.HasIndex(e => e.ChampionId, "IX_bans_champion");

            entity.HasIndex(e => e.TeamId, "IX_bans_team");

            entity.Property(e => e.IdBan).HasColumnName("id_ban");
            entity.Property(e => e.BanOrder).HasColumnName("ban_order");
            entity.Property(e => e.ChampionId).HasColumnName("champion_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");

            entity.HasOne(d => d.Champion).WithMany(p => p.Bans)
                .HasForeignKey(d => d.ChampionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bans_champion");

            entity.HasOne(d => d.Game).WithMany(p => p.Bans)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bans_game");

            entity.HasOne(d => d.Team).WithMany(p => p.Bans)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_bans_team");
        });

        modelBuilder.Entity<Champion>(entity =>
        {
            entity.HasKey(e => e.IdChampion).HasName("PK__champion__000A3023C2C73601");

            entity.ToTable("champions");

            entity.HasIndex(e => e.Name, "UQ__champion__72E12F1B1708EC56").IsUnique();

            entity.Property(e => e.IdChampion).HasColumnName("id_champion");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.IdGame).HasName("PK__games__0E819B2CE4C66960");

            entity.ToTable("games");

            entity.HasIndex(e => e.DatePlayed, "IX_games_date");

            entity.HasIndex(e => e.Patch, "IX_games_patch");

            entity.HasIndex(e => new { e.SeriesId, e.GameNumber }, "UQ_game_in_series").IsUnique();

            entity.Property(e => e.IdGame).HasColumnName("id_game");
            entity.Property(e => e.DatePlayed).HasColumnName("date_played");
            entity.Property(e => e.GameNumber).HasColumnName("game_number");
            entity.Property(e => e.Link)
                .HasMaxLength(255)
                .HasColumnName("link");
            entity.Property(e => e.Patch)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("patch");
            entity.Property(e => e.SeriesId).HasColumnName("series_id");

            entity.HasOne(d => d.Series).WithMany(p => p.Games)
                .HasForeignKey(d => d.SeriesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_games_series");
        });

        modelBuilder.Entity<GamePlayer>(entity =>
        {
            entity.HasKey(e => e.IdGamePlayer).HasName("PK__game_pla__563827FB5F5FE2BD");

            entity.ToTable("game_players");

            entity.HasIndex(e => e.ChampionId, "IX_game_players_champion");

            entity.HasIndex(e => e.PlayerId, "IX_game_players_player");

            entity.HasIndex(e => new { e.GameTeamId, e.PlayerId }, "UQ_game_team_player").IsUnique();

            entity.Property(e => e.IdGamePlayer).HasColumnName("id_game_player");
            entity.Property(e => e.ChampionId).HasColumnName("champion_id");
            entity.Property(e => e.GameTeamId).HasColumnName("game_team_id");
            entity.Property(e => e.PickOrder).HasColumnName("pick_order");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");

            entity.HasOne(d => d.Champion).WithMany(p => p.GamePlayers)
                .HasForeignKey(d => d.ChampionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_game_players_champion");

            entity.HasOne(d => d.GameTeam).WithMany(p => p.GamePlayers)
                .HasForeignKey(d => d.GameTeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_game_players_game_team");

            entity.HasOne(d => d.Player).WithMany(p => p.GamePlayers)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_game_players_player");
        });

        modelBuilder.Entity<GameTeam>(entity =>
        {
            entity.HasKey(e => e.IdGameTeam).HasName("PK__game_tea__C82B2E8A57FDC215");

            entity.ToTable("game_teams");

            entity.HasIndex(e => e.Result, "IX_game_teams_result");

            entity.HasIndex(e => e.TeamId, "IX_game_teams_team");

            entity.HasIndex(e => new { e.GameId, e.TeamId }, "UQ_game_team").IsUnique();

            entity.Property(e => e.IdGameTeam).HasColumnName("id_game_team");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Result)
                .HasMaxLength(4)
                .HasColumnName("result");
            entity.Property(e => e.Side)
                .HasMaxLength(4)
                .HasColumnName("side");
            entity.Property(e => e.TeamId).HasColumnName("team_id");

            entity.HasOne(d => d.Game).WithMany(p => p.GameTeams)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_game_teams_game");

            entity.HasOne(d => d.Team).WithMany(p => p.GameTeams)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_game_teams_team");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.IdPlayer).HasName("PK__players__45CF72B1BF8848FC");

            entity.ToTable("players");

            entity.HasIndex(e => e.TeamId, "IX_players_team_id");

            entity.HasIndex(e => e.IngameName, "UQ__players__2AF4ED2A4CF07C86").IsUnique();

            entity.Property(e => e.IdPlayer).HasColumnName("id_player");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasColumnName("country");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IngameName)
                .HasMaxLength(50)
                .HasColumnName("ingame_name");
            entity.Property(e => e.PhotoUrl)
                .HasMaxLength(255)
                .HasColumnName("photo_url");
            entity.Property(e => e.Position)
                .HasMaxLength(10)
                .HasColumnName("position");
            entity.Property(e => e.TeamId).HasColumnName("team_id");

            entity.HasOne(d => d.Team).WithMany(p => p.Players)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("FK_players_teams");
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasKey(e => e.IdSeries).HasName("PK__series__F4DBDFD5432DAAEC");

            entity.ToTable("series");

            entity.HasIndex(e => new { e.TournamentId, e.TeamOpponentId, e.MatchDate }, "UQ_series").IsUnique();

            entity.Property(e => e.IdSeries).HasColumnName("id_series");
            entity.Property(e => e.BestOf)
                .HasDefaultValue(3)
                .HasColumnName("best_of");
            entity.Property(e => e.MatchDate).HasColumnName("match_date");
            entity.Property(e => e.TeamOpponentId).HasColumnName("team_opponent_id");
            entity.Property(e => e.TeamT1Id).HasColumnName("team_t1_id");
            entity.Property(e => e.TournamentId).HasColumnName("tournament_id");

            entity.HasOne(d => d.TeamOpponent).WithMany(p => p.SeriesTeamOpponents)
                .HasForeignKey(d => d.TeamOpponentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_series_team_opponent");

            entity.HasOne(d => d.TeamT1).WithMany(p => p.SeriesTeamT1s)
                .HasForeignKey(d => d.TeamT1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_series_team_t1");

            entity.HasOne(d => d.Tournament).WithMany(p => p.Series)
                .HasForeignKey(d => d.TournamentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_series_tournament");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.IdTeam).HasName("PK__teams__C6D204E7D107974F");

            entity.ToTable("teams");

            entity.HasIndex(e => e.Name, "UQ__teams__72E12F1B0F5DA16F").IsUnique();

            entity.Property(e => e.IdTeam).HasColumnName("id_team");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(255)
                .HasColumnName("logo_url");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Region)
                .HasMaxLength(20)
                .HasColumnName("region");
        });

        modelBuilder.Entity<Tournament>(entity =>
        {
            entity.HasKey(e => e.IdTournament).HasName("PK__tourname__1471F11D2DF15622");

            entity.ToTable("tournaments");

            entity.HasIndex(e => new { e.Name, e.Year }, "UQ_tournaments_name_year").IsUnique();

            entity.Property(e => e.IdTournament).HasColumnName("id_tournament");
            entity.Property(e => e.IsT1winner)
                .HasMaxLength(10)
                .HasColumnName("isT1winner");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Region)
                .HasMaxLength(20)
                .HasDefaultValue("KR")
                .HasColumnName("region");
            entity.Property(e => e.Winner)
                .HasMaxLength(100)
                .HasColumnName("winner");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
