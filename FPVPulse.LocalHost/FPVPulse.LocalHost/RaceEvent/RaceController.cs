using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
			return race;
		}

		[HttpGet("event/{eventId}/raceIndex")]
		public IEnumerable<IndexEntry> GetEventRaceIndex(int eventId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			return db.Races.Where(e => e.EventId == eventId).Select(e => new IndexEntry
			{
				Id = e.EventId,
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
		public ActionResult<RacePilot> GetRacePilot(int racePilotId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var racePilot = db.RacePilots.Find(racePilotId);

			if (racePilot == null)
				return NotFound();
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
	}
}
