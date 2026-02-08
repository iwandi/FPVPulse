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
			var getEventId = db.Events.Where( e => e.InjestEventId == parentId).Select(e => e.EventId).FirstOrDefaultAsync();
			var getExistingRace = db.Races.Where(r => r.InjestRaceId == id).FirstOrDefaultAsync();

			await Task.WhenAll(getEventId, getExistingRace);

			var eventId = getEventId.Result;
			var existingRace = getExistingRace.Result;
			if (existingRace == null)
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
					var dataPilot = injestPilot as DbInjestRacePilot;
					if (dataPilot == null)
						throw new Exception("Unexpected type DbInjestRace contains non DbInjestRacePilot in Pilots");

					var pilotInjestId = dataPilot.InjestPilotId;
					var existingPilot = await db.MatchPilot(pilotInjestId, dataPilot.InjestName);

					var racePilotInjestId = dataPilot.RacePilotId;
					var existingRacePilot = await db.RacePilots.Where(rp => rp.InjestRacePilotId == racePilotInjestId).FirstOrDefaultAsync();
					if(existingRacePilot == null)
					{
						existingRacePilot = new RacePilot { InjestRacePilotId = racePilotInjestId, PilotId = existingPilot.PilotId };
						Merge(existingRacePilot, dataPilot);
						db.RacePilots.Add(existingRacePilot);
						await db.SaveChangesAsync();
					}
					else if(Merge(existingRacePilot, dataPilot))
						await db.SaveChangesAsync();
				}
			}
		}

		bool Merge(Race race, DbInjestRace injestRace)
		{
			bool hasChange = false;

			hasChange |= MergeUtil.MergeMemberString(ref race.Name, injestRace.InjestRaceId);
			hasChange |= MergeUtil.MergeEnumMember(ref race.RaceType, injestRace.RaceType);
			hasChange |= MergeUtil.MergeEnumMember(ref race.RaceLayout, injestRace.RaceLayout);
			hasChange |= MergeUtil.MergeMember(ref race.FirstOrderPoistion, injestRace.FirstOrderPoistion);
			hasChange |= MergeUtil.MergeMember(ref race.SecondOrderPosition, injestRace.SecondOrderPosition);

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