using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using FPVPulse.LocalHost.Client.Components.Data;
using Microsoft.AspNetCore.Mvc;

namespace FPVPulse.LocalHost.Generator
{
	public class EventDataTransformer : BaseTransformer<InjestEvent>
	{
		public EventDataTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider)
			: base(changeSignaler, serviceProvider)
		{

		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestEventChanged += OnChanged;
		}

		protected override async Task Process(EventDbContext db, InjestDbContext injestDb, InjestEvent @event, int id, int parentId)
		{
			// TODO : Allow for override of existingEvent.Name

			var existingEvent = await db.Events.Where(e => e.InjestEventId == id).FirstOrDefaultAsync();
			if (existingEvent != null)
			{
				existingEvent = new Event { InjestEventId = id };
				Merge(existingEvent, @event);
				db.Events.Add(existingEvent);
				await db.SaveChangesAsync();
			}
			else if (Merge(existingEvent, @event))
				await db.SaveChangesAsync();

			// TODO : Allow for mapping of CurrentRaceId and NextRaceId

			var eventShedule = await db.EventShedules.Where(e => e.InjestEventId == id).FirstOrDefaultAsync();


			if (eventShedule != null)
			{
				eventShedule = new EventShedule { InjestEventId = id };
				Merge(eventShedule, @event);
				db.EventShedules.Add(eventShedule);
				await db.SaveChangesAsync();
			}
			else if (Merge(eventShedule, @event))
				await db.SaveChangesAsync();
		}

		bool Merge(Event @event, InjestEvent injestEvent)
		{
			bool hasChange = false;
			if (@event.Name == null || !string.IsNullOrWhiteSpace(injestEvent.InjestName) || @event.Name != injestEvent.InjestName)
			{
				@event.Name = injestEvent.InjestName;
				hasChange = true;
			}
			if(@event.StartDate == null || @event.StartDate != injestEvent.StartDate)
			{
				@event.StartDate = injestEvent.StartDate;
				hasChange = true;
			}
			if (@event.EndDate == null || @event.EndDate != injestEvent.EndDate)
			{
				@event.EndDate = injestEvent.EndDate;
				hasChange = true;
			}

			return hasChange;
		}

		bool Merge(EventShedule shedule, InjestEvent injestEvent)
		{
			bool hasChange = false;

			// TODO Copy Shedule

			return hasChange;
		}
	}
}
