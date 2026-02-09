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
				WriteData(existingRace, data);
				db.Races.Add(existingRace);
			}
			else
				WriteData(existingRace, data);

			var raceHasChanges = await db.SaveChangesAsync() > 0;

			List<Pilot> changedPilot = new List<Pilot>();
			List<RacePilot> racePilots = new List<RacePilot>();
			List<RacePilot> changedRacePilot = new List<RacePilot>();
			if (data.Pilots != null && data.Pilots.Length > 0)
			{
				foreach (var injestPilot in data.Pilots)
				{
					var dataPilot = injestPilot as DbInjestRacePilot;
					if (dataPilot == null)
						throw new Exception("Unexpected type DbInjestRace contains non DbInjestRacePilot in Pilots");

					var pilotInjestId = dataPilot.InjestPilotId;
					var (existingPilot, pilotHasChanges) = await db.MatchPilot(pilotInjestId, dataPilot.InjestName);

					var racePilotInjestId = dataPilot.RacePilotId;
					var existingRacePilot = await db.RacePilots.Where(rp => rp.InjestRacePilotId == racePilotInjestId).FirstOrDefaultAsync();
					if (existingRacePilot == null)
					{
						existingRacePilot = new RacePilot { InjestRacePilotId = racePilotInjestId, PilotId = existingPilot.PilotId, EventId = eventId };
						WriteData(existingRacePilot, dataPilot);
						db.RacePilots.Add(existingRacePilot);
					}
					else
						WriteData(existingRacePilot, dataPilot);

					var racePilotHasChanges = await db.SaveChangesAsync() > 0;

					existingRacePilot.Pilot = existingPilot;
					if(pilotHasChanges)
						changedPilot.Add(existingPilot);
					if(racePilotHasChanges)
						changedRacePilot.Add(existingRacePilot);
				}
			}

			existingRace.Pilots = racePilots.ToArray();

			if(raceHasChanges)
				changeSignaler.SignalChangeAsync(ChangeGroup.Race, existingRace.RaceId, existingRace.EventId, existingRace);
			foreach (var pilot in changedPilot)
				changeSignaler.SignalChangeAsync(ChangeGroup.Pilot, pilot.PilotId, 0, pilot);
			foreach (var racePilot in changedRacePilot)
				changeSignaler.SignalChangeAsync(ChangeGroup.RacePilot, racePilot.RacePilotId, racePilot.PilotId, racePilot);
		}

		void WriteData(Race race, DbInjestRace injestRace)
		{
			race.Name = injestRace.InjestName ?? string.Empty;
			race.RaceType = injestRace.RaceType ?? RaceType.Unknown;
			race.RaceLayout = injestRace.RaceLayout ?? RaceLayout.Unknown;
			race.FirstOrderPoistion = injestRace.FirstOrderPoistion ?? 0;
			race.SecondOrderPosition = injestRace.SecondOrderPosition ?? 0;
		}

		void WriteData(RacePilot racePilot, DbInjestRacePilot injestRacePilot)
		{
			racePilot.SeedPosition = injestRacePilot.SeedPosition;
			racePilot.StartPosition = injestRacePilot.StartPosition ?? 0;
			racePilot.Position = injestRacePilot.Position ?? 0;
			racePilot.Channel = injestRacePilot.Channel ?? string.Empty;
		}
	}
}