using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using FPVPulse.LocalHost.Client.Components.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace FPVPulse.LocalHost.Generator
{
	public class RaceDataTransformer : BaseTransformer<DbInjestRace>
	{
		public RaceDataTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{
		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestRaceChanged += OnChanged;
		}

		protected override async Task Process(EventDbContext db, DbInjestRace data, int id, int parentId)
		{
			var eventId = await db.Events.Where( e => e.InjestEventId == parentId).Select(e => e.EventId).FirstOrDefaultAsync();
			var existingRace = db.Races.Where(r => r.InjestRaceId == id).FirstOrDefault();

			if (existingRace != null)
			{
				existingRace = new Race { InjestRaceId = id, EventId = eventId };
				Merge(existingRace, data);
				db.Races.Add(existingRace);
				await db.SaveChangesAsync();
			}
			else if (Merge(existingRace, data))
				await db.SaveChangesAsync();

			if (data.Pilots != null && data.Pilots.Length > 0)
			{
				foreach (var injestPilot in data.Pilots)
				{
					DbInjestRacePilot dataPilot = injestPilot as DbInjestRacePilot;
					if (dataPilot == null)
						throw new Exception("Unexpected type DbInjestRace contains non DbInjestRacePilot in Pilots");

					int pilotInjestId = dataPilot.RacePilotId;
					//todo : this is more complicated as we want to find cross event pilots !!!
					var existingPilot = await db.Pilots.Where(p => p.InjestPilotId == pilotInjestId).FirstOrDefaultAsync();
					if (existingPilot != null)
					{
						existingPilot = new Pilot { InjestPilotId = pilotInjestId };
						Merge(existingPilot, dataPilot);
						db.Pilots.Add(existingPilot);
						await db.SaveChangesAsync();
					}
					else if (Merge(existingPilot, dataPilot))
						await db.SaveChangesAsync();

				}
			}
		}

		bool Merge(Race race, DbInjestRace injestRace)
		{
			bool hasChange = false;

			hasChange |= MergeUtil.MergeMemberString(ref race.Name, injestRace.InjestRaceId);
			hasChange |= MergeUtil.MergeMember(ref race.RaceType, injestRace.RaceType);
			hasChange |= MergeUtil.MergeMember(ref race.RaceLayout, injestRace.RaceLayout);
			hasChange |= MergeUtil.MergeMember(ref race.FirstOrderPoistion, injestRace.FirstOrderPoistion);
			hasChange |= MergeUtil.MergeMember(ref race.SecondOrderPosition, injestRace.SecondOrderPosition);

			return hasChange;
		}

		bool Merge(Pilot pilot, DbInjestRacePilot injestRacePilot)
		{
			bool hasChange = false;

			hasChange |= MergeUtil.MergeMemberString(ref pilot.DisplayName, injestRacePilot.InjestName);

			return hasChange;
		}

		bool Merge(RacePilot racePilot, DbInjestRacePilot injestRacePilot)
		{
			bool hasChange = false;

			hasChange |= MergeUtil.MergeMember(ref racePilot.SeedPosition, injestRacePilot.SeedPosition);
			hasChange |= MergeUtil.MergeMember(ref racePilot.StartPosition, injestRacePilot.StartPosition);
			hasChange |= MergeUtil.MergeMember(ref racePilot.Position, injestRacePilot.Position);
			hasChange |= MergeUtil.MergeMemberString(ref racePilot.Channel, injestRacePilot.Channel);

			return hasChange;
		}
	}
}