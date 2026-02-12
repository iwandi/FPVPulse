using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using FPVPulse.LocalHost.Client.Components.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using FPVPulse.LocalHost.Injest;

namespace FPVPulse.LocalHost.Generator
{
	public class RaceDataTransformer : BaseTransformer<DbInjestRace>
	{
		RacePilotDataTransformer racePilotDataTransformer;
		PilotResultDataTransformer pilotResultDataTransformer;

		public RaceDataTransformer(RacePilotDataTransformer rt, PilotResultDataTransformer prt, ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{
			racePilotDataTransformer = rt;
			pilotResultDataTransformer = prt;
		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestRaceChanged += OnChanged;
		}

		protected override async Task CheckExisting(EventDbContext db, InjestDbContext injestDb)
		{
			foreach (var race in injestDb.Races)
			{
				await ProcessUntilDone(db, race, race.RaceId, race.EventId.Value);
			}
		}

		protected override async Task<bool> Process(EventDbContext db, DbInjestRace data, int id, int parentId)
		{
			var getEventId = db.Events.Where( e => e.InjestEventId == parentId).Select(e => e.EventId).FirstOrDefaultAsync();
			var getExistingRace = db.Races.Where(r => r.InjestRaceId == id).FirstOrDefaultAsync();

			await Task.WhenAll(getEventId, getExistingRace);

			if(getEventId.Result == null)
				return false;

			var eventId = getEventId.Result;
			var existingRace = getExistingRace.Result;
			if (existingRace == null)
			{
				existingRace = new Race { InjestRaceId = id, EventId = eventId };
				WriteData(existingRace, data);
				db.Races.Add(existingRace);
			}
			else
				WriteData(existingRace, data);

			var raceHasChanges = await db.SaveChangesAsync() > 0;

			await Task.WhenAll(
				racePilotDataTransformer.WaitForAllParentsDone(id),
				pilotResultDataTransformer.WaitForAllParentsDone(id));

			await EventDbContext.FillRace(db, existingRace);

			if (raceHasChanges)
				changeSignaler.SignalChangeAsync(ChangeGroup.Race, existingRace.RaceId, existingRace.EventId, existingRace);
			return true;
		}

		void WriteData(Race race, DbInjestRace injestRace)
		{
			var name = injestRace.InjestName;
			var shortName = name;
			if (injestRace.FirstOrderPoistion.HasValue && injestRace.SecondOrderPosition.HasValue)
			{
				if (injestRace.RaceType == RaceType.Qualifying)
				{
					name = $"Qualifying Bracket: {injestRace.FirstOrderPoistion} Heat: {injestRace.SecondOrderPosition}";
					shortName = $"Q{injestRace.FirstOrderPoistion}-{injestRace.SecondOrderPosition}";
				}
				else if (injestRace.RaceType == RaceType.Practice)
				{
					name = $"Practice Bracket: {injestRace.FirstOrderPoistion} Heat: {injestRace.SecondOrderPosition}";
					shortName = $"P{injestRace.FirstOrderPoistion}-{injestRace.SecondOrderPosition}";
				}
			}
			else if(injestRace.RaceType == RaceType.Mains)
			{
				if (injestRace.FirstOrderPoistion.HasValue)
				{
					name = $"Main Race: {injestRace.FirstOrderPoistion}";
					shortName = $"M{injestRace.FirstOrderPoistion}";
				}
				else if (injestRace.SecondOrderPosition.HasValue)
				{
					name = $"Main Race: {injestRace.SecondOrderPosition}";
					shortName = $"M{injestRace.SecondOrderPosition}";
				}
			}

			race.Name = name ?? string.Empty;
			race.ShortName = shortName ?? string.Empty;
			race.RaceType = injestRace.RaceType ?? RaceType.Unknown;
			race.RaceLayout = injestRace.RaceLayout ?? RaceLayout.Unknown;
			race.FirstOrderPoistion = injestRace.FirstOrderPoistion ?? 0;
			race.SecondOrderPosition = injestRace.SecondOrderPosition ?? 0;
		}
	}
}