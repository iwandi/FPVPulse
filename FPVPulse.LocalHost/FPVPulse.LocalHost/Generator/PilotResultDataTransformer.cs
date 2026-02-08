using FPVPulse.Ingest;
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
			var existingPilotResult = await db.RacePilotResults.Where(pr => pr.InjestPilotResultId == id).FirstOrDefaultAsync();
			var hasChanges = false;
			if (existingPilotResult == null)
			{
				existingPilotResult = new RacePilotResult { InjestPilotResultId = id };
				Merge(existingPilotResult, data);
				db.RacePilotResults.Add(existingPilotResult);
				hasChanges = true;
			}
			else 
				hasChanges |= Merge(existingPilotResult, data);


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
						Merge(existingLap, dataLap);
						existingPilotResult.Laps[i] = existingLap;
						hasChanges = true;
					}
					else
						hasChanges |= Merge(existingLap, dataLap);
				}

				var existingLapsCount = existingPilotResult.Laps?.Length ?? 0;
				if (existingLapsCount > dataLapsCount)
				{
					var copy = new Lap[dataLapsCount];
					Array.Copy(existingPilotResult.Laps, copy, dataLapsCount);
					existingPilotResult.Laps = copy;
					hasChanges = true;
				}
			}

			if (hasChanges)
				await db.SaveChangesAsync();
		}

		bool Merge(RacePilotResult racePilotResult, DbInjestPilotResult injestPilotResult)
		{
			bool hasChanges = false;

			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.Position, injestPilotResult.Position);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.CurrentSector, injestPilotResult.CurrentSector);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.CurrentSplit, injestPilotResult.CurrentSplit);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.StartTime, injestPilotResult.StartTime);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.FinishTime, injestPilotResult.FinishTime);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.LapCount, injestPilotResult.LapCount);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.TotalTime, injestPilotResult.TotalTime);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.TopLapTime, injestPilotResult.TopLapTime);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.Top2ConsecutiveLapTime, injestPilotResult.Top2ConsecutiveLapTime);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.Top3ConsecutiveLapTime, injestPilotResult.Top3ConsecutiveLapTime);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.AverageLapTime, injestPilotResult.AverageLapTime);
			hasChanges |= MergeUtil.MergeMember(ref racePilotResult.IsComplited, injestPilotResult.IsComplited);
			hasChanges |= MergeUtil.MergeEnumMember(ref racePilotResult.Flags, injestPilotResult.Flags);

			return hasChanges;
		}

		bool Merge(Lap lap, InjestLap injestLap)
		{
			bool hasChanges = false;

			hasChanges |= MergeUtil.MergeMember(ref lap.LapNumber, injestLap.LapNumber);
			hasChanges |= MergeUtil.MergeMember(ref lap.LapTime, injestLap.LapTime);
			hasChanges |= MergeUtil.MergeMember(ref lap.IsInvalid, injestLap.IsInvalid);

			return hasChanges;
		}
	}
}
