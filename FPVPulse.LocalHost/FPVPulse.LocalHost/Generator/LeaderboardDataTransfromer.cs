using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.EntityFrameworkCore;

namespace FPVPulse.LocalHost.Generator
{
	public class LeaderboardDataTransfromer : BaseTransformer<DbInjestLeaderboard>
	{
		public LeaderboardDataTransfromer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{
		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestLeaderabordChanged += OnChanged;
		}

		protected override async Task Process(EventDbContext db, DbInjestLeaderboard data, int id, int parentId)
		{
			var existingLeaderboard = await db.Leaderboards.Where(l => l.InjestLeaderboardId == id).FirstOrDefaultAsync();

			if(existingLeaderboard == null)
			{
				existingLeaderboard = new Leaderboard { InjestLeaderboardId = id };
				Merge(existingLeaderboard, data);
				db.Leaderboards.Add(existingLeaderboard);
				await db.SaveChangesAsync();
			}
			else if (Merge(existingLeaderboard, data))
				await db.SaveChangesAsync();
			
			if (data.Results != null && data.Results.Length > 0)
			{
				foreach (var injestResult in data.Results)
				{
					var dataPilot = injestResult as DbInjestLeaderboardPilot;
					if (dataPilot == null)
						throw new Exception("Unexpected type DbInjestLeaderboard contains non DbInjestLeaderboardPilot in Results");

					var injestPilotId = dataPilot.InjestPilotId;
					var existingPilot = await db.MatchPilot(injestPilotId, dataPilot.InjestName);

					var leaderboardPilotInjestId = dataPilot.LeaderboardPilotId;
					var existingLeaderboardPilot = await db.LeaderboardPilots.Where(lp => lp.InjestLeaderboardPilotId == leaderboardPilotInjestId).FirstOrDefaultAsync();
					if (existingLeaderboardPilot == null)
					{
						existingLeaderboardPilot = new LeaderboardPilot { InjestLeaderboardPilotId = leaderboardPilotInjestId, PilotId = existingPilot.PilotId };
						Merge(existingLeaderboardPilot, dataPilot, existingPilot.PilotId, parentId);
						db.LeaderboardPilots.Add(existingLeaderboardPilot);
						await db.SaveChangesAsync();
					}
					else if (Merge(existingLeaderboardPilot, dataPilot, existingPilot.PilotId, parentId))
						await db.SaveChangesAsync();
				}
			}
		}

		bool Merge(Leaderboard leaderboard, DbInjestLeaderboard injestLeaderboard)
		{
			bool changed = false;

			MergeUtil.MergeEnumMember(ref leaderboard.RaceType, injestLeaderboard.RaceType);

			return changed;
		}

		bool Merge(LeaderboardPilot leaderboardPilot, DbInjestLeaderboardPilot injestLeaderboardPilot, int pilotId, int positionReasonRaceId)
		{
			bool changed = false;

			changed |= MergeUtil.MergeMember(ref leaderboardPilot.PilotId, pilotId);
			changed |= MergeUtil.MergeMember(ref leaderboardPilot.Position, injestLeaderboardPilot.Position);
			changed |= MergeUtil.MergeMember(ref leaderboardPilot.PositionDelta, injestLeaderboardPilot.PositionDelta);
			changed |= MergeUtil.MergeMemberNullableString(ref leaderboardPilot.PositionReason, injestLeaderboardPilot.PositionReason);
			changed |= MergeUtil.MergeMember(ref leaderboardPilot.PositionReasonRaceId, positionReasonRaceId);
			changed |= MergeUtil.MergeEnumMember(ref leaderboardPilot.Flags, injestLeaderboardPilot.Flags);

			return changed;
		}
	}
}
