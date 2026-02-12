using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client.Components.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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

		public async Task<(Pilot,bool)> MatchPilot(string? injestPilotId, string? injestName)
		{
			var existingPilot = await Pilots.Where(p => p.InjestPilotId == injestPilotId).FirstOrDefaultAsync();

			if (existingPilot == null)
				existingPilot = await Pilots.Where(p => p.DisplayName == injestName).FirstOrDefaultAsync();
			// TODO : More ways to try to resolve the Pilot

			if (existingPilot == null)
			{
				existingPilot = new Pilot { InjestPilotId = injestPilotId };
				WriteData(existingPilot, injestName);
				Pilots.Add(existingPilot);
			}
			else
				WriteData(existingPilot, injestName);

			var hasChanged = await SaveChangesAsync() > 0;

			return (existingPilot, hasChanged);
		}

		void WriteData(Pilot pilot, string? injestName)
		{
			pilot.DisplayName = injestName ?? pilot.DisplayName;
		}

		public static async Task FillLeaderboard(EventDbContext db, Leaderboard leaderboard)
		{
			leaderboard.Pilots = await db.LeaderboardPilots.Where(lp => lp.LeaderboardId == leaderboard.LeaderboardId).ToArrayAsync();

			if (leaderboard.Pilots != null)
			{
				foreach (var pilot in leaderboard.Pilots)
				{
					pilot.Pilot = await db.Pilots.FindAsync(pilot.PilotId);
				}
			}
		}

		public static async Task FillEvent(EventDbContext db, Event @event)
		{
			@event.Shedule = await db.EventShedules.Where(e => e.EventId == @event.EventId).FirstOrDefaultAsync();

			@event.Races = await db.Races.Where(r => r.EventId == @event.EventId && !r.Invalid).ToArrayAsync();
			foreach (var race in @event.Races)
			{
				await FillRace(db, race);
			}

			@event.Leaderboards = await db.Leaderboards.Where(l => l.EventId == @event.EventId).ToArrayAsync();
			foreach (var leaderboard in @event.Leaderboards)
			{
				await FillLeaderboard(db, leaderboard);
			}
		}

		public static async Task FillRace(EventDbContext db, Race race)
		{
			race.Results = await db.RacePilotResults.Where(e => e.LazyRaceId == race.RaceId).ToArrayAsync();
			race.Pilots = await db.RacePilots.Where(e => e.RaceId == race.RaceId).ToArrayAsync();

			foreach (var pilot in race.Pilots)
			{
				await FillRacePilot(db, pilot);
			}
		}

		public static async Task FillRacePilot(EventDbContext db, RacePilot pilot)
		{
			pilot.Pilot = await db.Pilots.Where(p => p.PilotId == pilot.PilotId).FirstOrDefaultAsync();
		}
	}
}
