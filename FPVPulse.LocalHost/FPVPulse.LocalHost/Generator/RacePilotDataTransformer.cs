using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.EntityFrameworkCore;

namespace FPVPulse.LocalHost.Generator
{
	public class RacePilotDataTransformer : BaseTransformer<DbInjestRacePilot>
	{
		public RacePilotDataTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{
		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestRacePilotChanged += OnChanged;
		}

		protected override async Task CheckExisting(EventDbContext db, InjestDbContext injestDb)
		{
			foreach (var racePilot in injestDb.RacePilots)
			{
				await ProcessUntilDone(db, racePilot, racePilot.RacePilotId, racePilot.RaceId);
			}
		}

		protected override async Task<bool> Process(EventDbContext db, DbInjestRacePilot data, int id, int parentId)
		{
			var existingRace = await db.Races.Where(r => r.InjestRaceId == parentId).FirstOrDefaultAsync();
			if(existingRace == null)
				return false;

			var pilotInjestId = data.InjestPilotId;
			var (existingPilot, pilotHasChanges) = await db.MatchPilot(pilotInjestId, data.InjestName);

			var racePilotInjestId = data.RacePilotId;
			var existingRacePilot = await db.RacePilots.Where(rp => rp.InjestRacePilotId == racePilotInjestId).FirstOrDefaultAsync();
			if (existingRacePilot == null)
			{
				existingRacePilot = new RacePilot { InjestRacePilotId = racePilotInjestId, PilotId = existingPilot.PilotId, EventId = existingRace.EventId, RaceId = existingRace.RaceId };
				WriteData(existingRacePilot, data);
				db.RacePilots.Add(existingRacePilot);
			}
			else
				WriteData(existingRacePilot, data);

			var racePilotHasChanges = await db.SaveChangesAsync() > 0;

			existingRacePilot.Pilot = existingPilot;

			if(pilotHasChanges)
				changeSignaler.SignalChangeAsync(ChangeGroup.Pilot, existingPilot.PilotId, 0, existingPilot);
			if(racePilotHasChanges)
				changeSignaler.SignalChangeAsync(ChangeGroup.RacePilot, existingRacePilot.RacePilotId, existingRacePilot.PilotId, existingRacePilot);
			return true;
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
