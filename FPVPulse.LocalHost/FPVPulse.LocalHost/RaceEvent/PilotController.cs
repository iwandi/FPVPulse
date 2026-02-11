using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FPVPulse.LocalHost.RaceEvent
{
	[Route("api")]
	[ApiController]
	public class PilotController : ControllerBase
	{
		readonly IServiceProvider serviceProvider;

		public PilotController(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		[HttpGet("pilot/{pilotId}")]
		public ActionResult<Pilot> GetPilot(int pilotId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var pilot = db.Pilots.Find(pilotId);

			if (pilot == null)
				return NotFound();
			return pilot;
		}

		[HttpGet("pilotIndex/")]
		public IEnumerable<IndexEntry> GetPilotIndex()
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			return db.Pilots.Select(e => new IndexEntry
			{
				Id = e.PilotId,
				Name = e.DisplayName,
			}).ToArray();
		}

		[HttpGet("event/{eventId}/pilotIndex/")]
		public IEnumerable<IndexEntry> GetEventPilots(int eventId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var pilotList = (
				from pilot in db.Pilots
				join racePilot in db.RacePilots on pilot.PilotId equals racePilot.PilotId
				where racePilot.EventId == eventId
				select new IndexEntry
				{
					Id = pilot.PilotId,
					Name = pilot.DisplayName
				}
			).Distinct().ToArray();
			return pilotList;
		}

		[HttpGet("pilot/{pilotId}/eventIndex")]
		public ActionResult<IEnumerable<IndexEntry>> GetPilotEventIndex(int pilotId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var eventList = (
				from racePilot in db.RacePilots
				join ev in db.Events on racePilot.EventId equals ev.EventId
				where racePilot.PilotId == pilotId
				select new IndexEntry
				{
					Id = ev.EventId,
					Name = ev.Name
				}
			).Distinct().ToArray();

			return eventList;
		}
	}
}
