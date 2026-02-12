using FPVPulse.Ingest;
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

			EventDbContext.FillRace(db, race).Wait();

			return race;
		}

		[HttpGet("event/{eventId}/raceIndex")]
		public IEnumerable<IndexEntry> GetEventRaceIndex(int eventId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			return db.Races.Where(e => e.EventId == eventId && e.Invalid == false)
				.OrderBy(e =>
					e.RaceType == RaceType.Practice ? 0 :
					e.RaceType == RaceType.Qualifying ? 1 :
					e.RaceType == RaceType.Mains ? 2 :
					e.RaceType == RaceType.Unknown ? 3 : 99)
				.ThenBy(e => e.SecondOrderPosition)
				.ThenBy(e => e.SecondOrderPosition)
				.Select(e => new IndexEntry
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
				where rp.RaceId == raceId && rp.Invalid == false
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

			await EventDbContext.FillRacePilot(db, racePilot);

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

		[HttpGet("event/{eventId}/pilot/{pilotId}/raceIndex")]
		public IEnumerable<IndexEntry> GetRacePilotRaceIndex(int eventId, int pilotId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var pilotIndex = (
				from rp in db.RacePilots
				join r in db.Races on rp.RaceId equals r.RaceId
				where rp.PilotId == pilotId && rp.EventId == eventId && r.Invalid == false
				select new IndexEntry
				{
					Id = r.RaceId,
					Name = r.Name
				}
			).Distinct().ToArray();

			return pilotIndex;
		}
	}
}
