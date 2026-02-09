using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FPVPulse.LocalHost.RaceEvent
{
	[Route("api")]
	[ApiController]
	public class EventController : ControllerBase
	{
		readonly IServiceProvider serviceProvider;

		public EventController(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		[HttpGet("event/{eventId}")]
		public ActionResult<Event> GetEvent(int eventId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var @event = db.Events.Find(eventId);

			if (@event == null)
				return NotFound();
			return @event;
		}

		[HttpGet("eventIndex/")]
		public IEnumerable<IndexEntry> GetEventIndex()
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			return db.Events.OrderBy(e => e.StartDate).Select(e => new IndexEntry
			{
				Id = e.EventId,
				Name = e.Name,
			}).ToArray();
		}

		[HttpGet("event/{eventId}/shedule")]
		public ActionResult<EventShedule> GetEventShedule(int eventId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var shedule = db.EventShedules.OrderBy(e => e.EventId == eventId).FirstOrDefault();

			if(shedule == null)
				return NotFound();

			return shedule;
		}
	}
}
