using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.EntityFrameworkCore;

namespace FPVPulse.LocalHost.Generator
{
	public class PilotResultDataTransformer : BaseTransformer<DbInjestPilotResult>
	{
		public PilotResultDataTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{

		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestPilotResultChanged += OnChanged;
		}

		protected override async Task Process(EventDbContext db, DbInjestPilotResult data, int id, int parentId)
		{
			var getRacePilotId = db.RacePilots.Where(e => e.InjestRacePilotId == parentId).Select(e => new { e.RaceId, e.PilotId }).FirstOrDefaultAsync();
			var getExistingPilot = db.RacePilotResults.Where(pr => pr.InjestPilotResultId == id).FirstOrDefaultAsync();

			await Task.WhenAll(getRacePilotId, getExistingPilot);

			var racePilotResult = getRacePilotId.Result;
			var existingPilotResult = getExistingPilot.Result;

			if (existingPilotResult == null)
			{
				existingPilotResult = new RacePilotResult { LazyRaceId = racePilotResult?.RaceId, LazyRacePilotId = racePilotResult?.PilotId, InjestPilotResultId = id };
				WriteData(existingPilotResult, data);
				db.RacePilotResults.Add(existingPilotResult);
			}
			else 
				WriteData(existingPilotResult, data);

			var dataLapsCount = data.Laps != null ? data.Laps.Length : 0;
			if (dataLapsCount == 0)
				existingPilotResult.Laps = null;
			else 
			{
				if (existingPilotResult.Laps == null)
					existingPilotResult.Laps = new Lap[dataLapsCount];

				for (int i = 0; i < dataLapsCount; i++)
				{
					var dataLap = data.Laps[i];
					var existingLap = existingPilotResult.Laps[i];
					if (existingLap == null)
					{
						existingLap = new Lap();
						WriteData(existingLap, dataLap);
						existingPilotResult.Laps[i] = existingLap;
					}
					else
						WriteData(existingLap, dataLap);
				}

				var existingLapsCount = existingPilotResult.Laps?.Length ?? 0;
				if (existingLapsCount > dataLapsCount)
				{
					var copy = new Lap[dataLapsCount];
					Array.Copy(existingPilotResult.Laps, copy, dataLapsCount);
					existingPilotResult.Laps = copy;
				}
			}

			if(data.RacePilotId == 0)
				data.RacePilotId = null;

			var raceHasChanges = await db.SaveChangesAsync() > 0;

			if(raceHasChanges)
				await changeSignaler.SignalChangeAsync(ChangeGroup.RacePilotResult, existingPilotResult.RacePilotResultId, existingPilotResult.LazyRacePilotId ?? -1, existingPilotResult);
		}

		void WriteData(RacePilotResult racePilotResult, DbInjestPilotResult injestPilotResult)
		{
			racePilotResult.Position = injestPilotResult.Position;
			racePilotResult.CurrentSector = injestPilotResult.CurrentSector;
			racePilotResult.CurrentSplit = injestPilotResult.CurrentSplit;
			racePilotResult.StartTime = injestPilotResult.StartTime;
			racePilotResult.FinishTime = injestPilotResult.FinishTime;
			racePilotResult.LapCount = injestPilotResult.LapCount;
			racePilotResult.TotalTime = injestPilotResult.TotalTime;
			racePilotResult.TopLapTime = injestPilotResult.TopLapTime;
			racePilotResult.Top2ConsecutiveLapTime = injestPilotResult.Top2ConsecutiveLapTime;
			racePilotResult.Top3ConsecutiveLapTime = injestPilotResult.Top3ConsecutiveLapTime;
			racePilotResult.AverageLapTime = injestPilotResult.AverageLapTime;
			racePilotResult.IsComplited = injestPilotResult.IsComplited;
			racePilotResult.Flags = injestPilotResult.Flags;
		}

		void WriteData(Lap lap, InjestLap injestLap)
		{
			lap.LapNumber = injestLap.LapNumber;
			lap.LapTime = injestLap.LapTime;
			lap.IsInvalid = injestLap.IsInvalid;
		}
	}
}
