using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FPVPulse.LocalHost.RaceEvent
{
	[Route("api")]
	[ApiController]
	public class RaceController : ControllerBase
	{
		readonly IServiceProvider serviceProvider;

		public RaceController(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		[HttpGet("race/{raceId}")]
		public ActionResult<Race> GetRace(int raceId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var race = db.Races.Find(raceId);

			if (race == null)
				return NotFound();

			FillRace(db, race).Wait();

			return race;
		}

		[HttpGet("event/{eventId}/raceIndex")]
		public IEnumerable<IndexEntry> GetEventRaceIndex(int eventId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			return db.Races.Where(e => e.EventId == eventId).Select(e => new IndexEntry
			{
				Id = e.RaceId,
				Name = e.Name,
			}).ToArray();
		}

		[HttpGet("race/{raceId}/pilotIndex")]
		public IEnumerable<IndexEntry> GetRacePilotIndex(int raceId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var pilotIndex = (
				from rp in db.RacePilots
				join p in db.Pilots on rp.PilotId equals p.PilotId
				where rp.RaceId == raceId
				select new IndexEntry
				{
					Id = p.PilotId,
					Name = p.DisplayName
				}
			).Distinct().ToArray();

			return pilotIndex;
		}

		[HttpGet("racePilot/{racePilotId}")]
		public async Task<ActionResult<RacePilot>> GetRacePilot(int racePilotId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var racePilot = db.RacePilots.Find(racePilotId);

			if (racePilot == null)
				return NotFound();

			FillRacePilot(db, racePilot).Wait();

			return racePilot;
		}

		[HttpGet("racePilotResult/{racePilotResultId}")]
		public ActionResult<RacePilotResult> GetRacePilotResult(int racePilotResultId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var result = db.RacePilotResults.Find(racePilotResultId);

			if (result == null)
				return NotFound();
			return result;
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
