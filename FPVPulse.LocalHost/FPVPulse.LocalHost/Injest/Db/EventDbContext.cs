using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client.Components.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace FPVPulse.LocalHost.Injest.Db
{
	public class EventDbContext : DbContext
	{
		public DbSet<Event> Events { get; set; }
		public DbSet<EventShedule> EventShedules { get; set; }
		public DbSet<Race> Races { get; set; }
		public DbSet<RacePilot> RacePilots { get; set; }
		public DbSet<RacePilotResult> RacePilotResults { get; set; }
		public DbSet<Pilot> Pilots { get; set; }
		public DbSet<Leaderboard> Leaderboards { get; set; }
		public DbSet<LeaderboardPilot> LeaderboardPilots { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlite("Data Source=event.db");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			var lapsConverter = new ValueConverter<Lap[]?, string?>(
				v => v == null ? null : JsonConvert.SerializeObject(v),
				v => v == null ? null : JsonConvert.DeserializeObject<Lap[]>(v)
			);

			modelBuilder.Entity<RacePilotResult>()
				.Property(e => e.Laps)
				.HasConversion(lapsConverter);
		}

		public async Task<Pilot> MatchPilot(string? injestPilotId, string? injestName)
		{
			var existingPilot = await Pilots.Where(p => p.InjestPilotId == injestPilotId).FirstOrDefaultAsync();

			if (existingPilot == null)
				existingPilot = await Pilots.Where(p => p.DisplayName == injestName).FirstOrDefaultAsync();
			// TODO : More ways to try to resolve the Pilot

			if (existingPilot == null)
			{
				existingPilot = new Pilot { InjestPilotId = injestPilotId };
				Merge(existingPilot, injestName);
				Pilots.Add(existingPilot);
				await SaveChangesAsync();
			}
			else if (Merge(existingPilot, injestName))
				await SaveChangesAsync();

			return existingPilot;
		}

		bool Merge(Pilot pilot, string? injestName)
		{
			bool hasChange = false;

			hasChange |= MergeUtil.MergeMemberString(ref pilot.DisplayName, injestName);

			return hasChange;
		}
	}
}
