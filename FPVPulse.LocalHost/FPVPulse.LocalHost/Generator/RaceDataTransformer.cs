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
		public RaceDataTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{
		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestRaceChanged += OnChanged;
		}

		protected override async Task CheckExisting(EventDbContext db, InjestDbContext injestDb)
		{
			foreach (var race in injestDb.Races)
			{
				while(!await Process(db, race, race.RaceId, race.EventId.Value))
				{

				}
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

			// TODO : Fill Result and Pilots

			if(raceHasChanges)
				changeSignaler.SignalChangeAsync(ChangeGroup.Race, existingRace.RaceId, existingRace.EventId, existingRace);
			return true;
		}

		void WriteData(Race race, DbInjestRace injestRace)
		{
			race.Name = injestRace.InjestName ?? string.Empty;
			race.RaceType = injestRace.RaceType ?? RaceType.Unknown;
			race.RaceLayout = injestRace.RaceLayout ?? RaceLayout.Unknown;
			race.FirstOrderPoistion = injestRace.FirstOrderPoistion ?? 0;
			race.SecondOrderPosition = injestRace.SecondOrderPosition ?? 0;
		}
	}
}