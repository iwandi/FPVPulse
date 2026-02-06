using FPVPulse.Ingest;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace FPVPulse.LocalHost.Injest.Db
{
    public class InjestDbContext : DbContext
    {
        public DbSet<DbInjestEvent> Events { get; set; }
        public DbSet<DbInjestRace> Races { get; set; }
        public DbSet<DbInjestRacePilot> RacePilots { get; set; }
        public DbSet<DbInjestPilotResult> PilotResults { get; set; }
		public DbSet<DbInjestLeaderboard> Leaderboard { get; set; }
		public DbSet<DbInjestLeaderboardPilot> LeaderboardPilots { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=injest.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<InjestEvent>();
            modelBuilder.Ignore<InjestRace>();
            modelBuilder.Ignore<InjestRacePilot>();
            modelBuilder.Ignore<InjestPilotResult>();
            modelBuilder.Ignore<InjestLap>();
			modelBuilder.Ignore<InjestLeaderboard>();
			modelBuilder.Ignore<InjestLeaderboardPilot>();

			var lapsConverter = new ValueConverter<InjestLap[]?, string?>(
                v => v == null ? null : JsonConvert.SerializeObject(v),
                v => v == null ? null : JsonConvert.DeserializeObject<InjestLap[]>(v)
            );

            modelBuilder.Entity<DbInjestPilotResult>()
                .Property(e => e.Laps)
                .HasConversion(lapsConverter);

            modelBuilder.Entity<DbInjestRacePilot>()
                .HasIndex(e => new { e.InjestId, e.InjestRaceId, e.InjestPilotEntryId })
                .IsUnique();

            modelBuilder.Entity<DbInjestRacePilot>()
                 .HasKey(p => p.RacePilotId);

			modelBuilder.Entity<DbInjestLeaderboardPilot>()
				.HasIndex(e => new { e.InjestId, e.InjestLeaderboardId, e.InjestPilotEntryId })
				.IsUnique();

			modelBuilder.Entity<DbInjestLeaderboardPilot>()
				 .HasKey(p => p.LeaderboardPilotId);
		}
    }
}
