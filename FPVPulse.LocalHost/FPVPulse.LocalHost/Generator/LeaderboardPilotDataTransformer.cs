using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.EntityFrameworkCore;

namespace FPVPulse.LocalHost.Generator
{
	public class LeaderboardPilotDataTransformer : BaseTransformer<DbInjestLeaderboardPilot>
	{
		public LeaderboardPilotDataTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{

		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestLeaderboardPilotChanged += OnChanged;
		}

		protected override async Task CheckExisting(EventDbContext db, InjestDbContext injestDb)
		{
			foreach (var leaderboardPilot in injestDb.LeaderboardPilots)
			{
				await ProcessUntilDone(db, leaderboardPilot, leaderboardPilot.LeaderboardPilotId, leaderboardPilot.LeaderboardId);
			}
		}

		protected override async Task<bool> Process(EventDbContext db, DbInjestLeaderboardPilot data, int id, int parentId)
		{
			var existingLeaderboard = await db.Leaderboards.Where(e => e.InjestLeaderboardId == parentId).FirstOrDefaultAsync();
			if(existingLeaderboard == null)
				return false;

			var injestPilotId = data.InjestPilotId;
			var (existingPilot, pilotHasChanged) = await db.MatchPilot(injestPilotId, data.InjestName);

			var leaderboardPilotInjestId = data.LeaderboardPilotId;
			var existingLeaderboardPilot = await db.LeaderboardPilots.Where(lp => lp.InjestLeaderboardPilotId == leaderboardPilotInjestId).FirstOrDefaultAsync();
			if (existingLeaderboardPilot == null)
			{
				existingLeaderboardPilot = new LeaderboardPilot { LeaderboardId = existingLeaderboard.LeaderboardId, InjestLeaderboardPilotId = leaderboardPilotInjestId, PilotId = existingPilot.PilotId };
				WriteData(existingLeaderboardPilot, data, existingPilot.PilotId, parentId);
				db.LeaderboardPilots.Add(existingLeaderboardPilot);
			}
			else
				WriteData(existingLeaderboardPilot, data, existingPilot.PilotId, parentId);

			var leaderboardPilotHasChanges = await db.SaveChangesAsync() > 0;

			existingLeaderboardPilot.Pilot = existingPilot;

			if(pilotHasChanged)
				await changeSignaler.SignalChangeAsync(ChangeGroup.Pilot, existingPilot.PilotId, 0, existingPilot);			

			await changeSignaler.SignalChangeAsync(ChangeGroup.LeaderboardPilot, existingLeaderboardPilot.LeaderboardPilotId, existingLeaderboardPilot.LeaderboardId, existingLeaderboardPilot);

			return true;
		}

		void WriteData(LeaderboardPilot leaderboardPilot, DbInjestLeaderboardPilot injestLeaderboardPilot, int pilotId, int positionReasonRaceId)
		{
			leaderboardPilot.PilotId = pilotId;
			leaderboardPilot.Position = injestLeaderboardPilot.Position;
			leaderboardPilot.PositionDelta = injestLeaderboardPilot.PositionDelta;
			leaderboardPilot.PositionReason = injestLeaderboardPilot.PositionReason;
			leaderboardPilot.PositionReasonRaceId = positionReasonRaceId;
			leaderboardPilot.Flags = injestLeaderboardPilot.Flags;
		}
	}
}
