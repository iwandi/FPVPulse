using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace FPVPulse.LocalHost.Generator
{
	public class RaceValidCheckTransformer : BaseTransformer<Race>
	{
		readonly ILogger<RaceValidCheckTransformer> logger;

		public RaceValidCheckTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{
			logger = serviceProvider.GetRequiredService<ILogger<RaceValidCheckTransformer>>();
		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnRaceChanged += OnChanged;
		}

		protected override async Task CheckExisting(EventDbContext db, InjestDbContext injestDb)
		{
			foreach (var race in db.Races)
			{
				await ProcessUntilDone(db, race, race.RaceId, race.EventId);
			}
		}

		protected override async Task<bool> Process(EventDbContext db, Race data, int id, int parentId)
		{
			bool invalid = false;

			if (data.RaceType == Ingest.RaceType.Unknown)
				invalid = true;
			else if(data.RaceType == Ingest.RaceType.Qualifying && (data.FirstOrderPoistion == 0 || data.SecondOrderPosition == 0))
				invalid = true;
			else if (data.RaceType == Ingest.RaceType.Practice && (data.FirstOrderPoistion == 0 || data.SecondOrderPosition == 0))
				invalid = true;
			else if (data.RaceType == Ingest.RaceType.Mains && (data.FirstOrderPoistion == 0 && data.SecondOrderPosition == 0))
				invalid = true;

			data.Invalid = invalid;

			var hasChanges = await db.SaveChangesAsync() > 0;

			// DANGER : this is a selve invokation.
			/*if (hasChanges)
				await changeSignaler.SignalChangeAsync(Client.ChangeGroup.Race, id, parentId, data);*/

			await CheckSequence(db, data);

			return true;
		}

		async Task CheckSequence(EventDbContext db, Race data)
		{
			logger.LogInformation($"Check Sequence: {data.RaceId}");

			var raceSequence = await db.Races.Where(e => e.EventId == data.EventId && e.RaceType == RaceType.Mains)
				.OrderByDescending(e => e.FirstOrderPoistion)
				.ThenByDescending(e => e.SecondOrderPosition)
				.ThenByDescending(e => e.RaceId)
				.Select(e => new { e.RaceId, e.FirstOrderPoistion, e.SecondOrderPosition, e.RaceLayout }).ToListAsync();

			if (raceSequence == null || raceSequence.Count <= 1)
				return;

			var infos = raceSequence.ToArray();

			var lastRace = infos.FirstOrDefault();
			RaceLayout raceLayot = lastRace.RaceLayout;
			int raceCount = lastRace.FirstOrderPoistion > lastRace.SecondOrderPosition ? lastRace.FirstOrderPoistion : lastRace.SecondOrderPosition;

			int bottomValidId = 0;
			int nextExpectedId = raceCount;
			HashSet<int> invalidIds = new ();
			foreach (var raceInfo in infos)
			{
				if(nextExpectedId <= 0)
				{
					invalidIds.Add(raceInfo.RaceId);
					continue;
				}
				if (raceLayot != raceInfo.RaceLayout)
				{
					invalidIds.Add(raceInfo.RaceId);
					continue;
				}
				if(raceInfo.FirstOrderPoistion == nextExpectedId || raceInfo.SecondOrderPosition == nextExpectedId)
				{
					nextExpectedId--;
					bottomValidId = raceInfo.RaceId;
				}
				else
				{
					invalidIds.Add(raceInfo.RaceId);
				}
			}


			logger.LogInformation($"Check stats: infos.Length:{infos.Length}, topValidId:{raceCount}, bottomValidId:{bottomValidId}, invalidIds.Count:{invalidIds.Count}");

			bool hasChange = false;
			foreach (var raceInfo in infos)
			{
				bool invalid = invalidIds.Contains(raceInfo.RaceId);
				var race = await db.Races.Where( e => e.RaceId == raceInfo.RaceId && e.Invalid != invalid).FirstOrDefaultAsync();
				if (race != null)
				{
					race.Invalid = invalid;
					hasChange = true;
				}
			}

			if (hasChange)
			{
				var hasChanges = await db.SaveChangesAsync() > 0;
			}
		}
	}
}
