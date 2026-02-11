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
	public class EventDataTransformer : BaseTransformer<DbInjestEvent>
	{
		public EventDataTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider)
			: base(changeSignaler, serviceProvider)
		{

		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestEventChanged += OnChanged;
		}

		protected override async Task CheckExisting(EventDbContext db, InjestDbContext injestDb)
		{
			foreach(var injestEvent in injestDb.Events)
			{
				while(!await Process(db, injestEvent, injestEvent.EventId, 0))
				{

				}
			}
		}

		protected override async Task<bool> Process(EventDbContext db, DbInjestEvent @event, int id, int parentId)
		{
			// TODO : Allow for override of existingEvent.Name

			var existingEvent = await db.Events.Where(e => e.InjestEventId == id).FirstOrDefaultAsync();
			if (existingEvent == null)
			{
				existingEvent = new Event { InjestEventId = id };
				WriteData(existingEvent, @event);
				db.Events.Add(existingEvent);
			}
			else
				WriteData(existingEvent, @event);
			var eventHasChange = await db.SaveChangesAsync() > 0;

			var getCurrentRaceInjestRaceId = db.Races.Where(e => e.InjestRaceId == @event.CurrentRaceId).Select(e => e.RaceId).FirstOrDefaultAsync();
			var getNextRaceInjestRaceId = db.Races.Where( e => e.InjestRaceId == @event.NextRaceId).Select(e => e.RaceId).FirstOrDefaultAsync();
			var getEventShedule = db.EventShedules.Where(e => e.InjestEventId == id).FirstOrDefaultAsync();

			await Task.WhenAll(getCurrentRaceInjestRaceId, getNextRaceInjestRaceId, getEventShedule);

			var currentRaceId = getCurrentRaceInjestRaceId.Result;
			var nextRaceId = getNextRaceInjestRaceId.Result;
			var eventShedule = getEventShedule.Result;

			if (eventShedule == null)
			{
				eventShedule = new EventShedule { InjestEventId = id, EventId = existingEvent.EventId };
				WriteData(eventShedule, @event, currentRaceId, nextRaceId);
				db.EventShedules.Add(eventShedule);
			}
			else
				WriteData(eventShedule, @event, currentRaceId, nextRaceId);
			
			var sheduleHasChange = await db.SaveChangesAsync() > 0;

			if(eventHasChange)
				await changeSignaler.SignalChangeAsync(ChangeGroup.Event, existingEvent.EventId, 0, existingEvent);
			if (sheduleHasChange)
				await changeSignaler.SignalChangeAsync(ChangeGroup.EventShedule, eventShedule.EventSheduleId, eventShedule.EventId, eventShedule);

			return true;
		}

		void WriteData(Event @event, InjestEvent injestEvent)
		{
			@event.Name = injestEvent.InjestName ?? string.Empty;
			@event.StartDate = injestEvent.StartDate;
			@event.EndDate = injestEvent.EndDate;
		}

		void WriteData(EventShedule shedule, InjestEvent injestEvent, int? currentRaceId, int? nextRaceId)
		{
			// TODO Copy Shedule

			shedule.CurrentRaceId = currentRaceId;
			shedule.NextRaceId = nextRaceId;
		}
	}
}
