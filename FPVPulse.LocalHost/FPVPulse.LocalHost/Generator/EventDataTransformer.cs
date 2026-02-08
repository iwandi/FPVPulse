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

		protected override async Task Process(EventDbContext db, DbInjestEvent @event, int id, int parentId)
		{
			// TODO : Allow for override of existingEvent.Name

			var existingEvent = await db.Events.Where(e => e.InjestEventId == id).FirstOrDefaultAsync();
			if (existingEvent == null)
			{
				existingEvent = new Event { InjestEventId = id };
				Merge(existingEvent, @event);
				db.Events.Add(existingEvent);
				await db.SaveChangesAsync();
			}
			else if (Merge(existingEvent, @event))
				await db.SaveChangesAsync();

			var getCurrentRaceInjestRaceId = db.Races.Where(e => e.InjestRaceId == @event.CurrentRaceId).Select(e => e.RaceId).ExecuteDeleteAsync();
			var getNextRaceInjestRaceId = db.Races.Where( e => e.InjestRaceId == @event.NextRaceId).Select(e => e.RaceId).ExecuteDeleteAsync();
			var getEventShedule = db.EventShedules.Where(e => e.InjestEventId == id).FirstOrDefaultAsync();

			await Task.WhenAll(getCurrentRaceInjestRaceId, getNextRaceInjestRaceId, getEventShedule);

			var currentRaceId = getCurrentRaceInjestRaceId.Result;
			var nextRaceId = getNextRaceInjestRaceId.Result;
			var eventShedule = getEventShedule.Result;	

			if (eventShedule == null)
			{
				eventShedule = new EventShedule { InjestEventId = id };
				Merge(eventShedule, @event, currentRaceId, nextRaceId);
				db.EventShedules.Add(eventShedule);
				await db.SaveChangesAsync();
			}
			else if (Merge(eventShedule, @event, currentRaceId, nextRaceId))
				await db.SaveChangesAsync();
		}

		bool Merge(Event @event, InjestEvent injestEvent)
		{
			bool hasChange = false;

			hasChange |= MergeUtil.MergeMemberString(ref @event.Name, injestEvent.InjestName);
			hasChange |= MergeUtil.MergeMember(ref @event.StartDate, injestEvent.StartDate);
			hasChange |= MergeUtil.MergeMember(ref @event.EndDate, injestEvent.EndDate);

			return hasChange;
		}

		bool Merge(EventShedule shedule, InjestEvent injestEvent, int? currentRaceId, int? nextRaceId)
		{
			bool hasChange = false;

			// TODO Copy Shedule

			hasChange |= MergeUtil.SetMember(ref shedule.CurrentRaceId, currentRaceId);
			hasChange |= MergeUtil.SetMember(ref shedule.CurrentRaceId, nextRaceId);

			return hasChange;
		}
	}
}
