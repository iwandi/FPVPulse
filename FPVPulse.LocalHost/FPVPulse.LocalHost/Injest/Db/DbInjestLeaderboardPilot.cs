using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;

namespace FPVPulse.LocalHost.Injest.Db
{
	public class DbInjestLeaderboardPilot : InjestLeaderboardPilot
	{
		[Key]
		public int LeaderboardPilotId { get; set; }
		[Required]
		public int LeaderboardId { get; set; }

		[Required]
		public string InjestId { get; set; } = string.Empty;
		[Required]
		public string InjestLeaderboardId { get; set; } = string.Empty;

		public int? PositionReasonRaceId;

		public DbInjestLeaderboardPilot()
		{

		}

		public DbInjestLeaderboardPilot(string injestId, InjestLeaderboardPilot leaderboardPilot, DbInjestLeaderboard leaderboard)
		{
			LeaderboardId = leaderboard.LeaderboardId;

			InjestId = injestId;
			InjestPilotEntryId = leaderboardPilot.InjestPilotEntryId;
			InjestPilotId = leaderboardPilot.InjestPilotId;

			InjestName = leaderboardPilot.InjestName;
			Position = leaderboardPilot.Position;
			PositionDelta = leaderboardPilot.PositionDelta;
			PositionReason = leaderboardPilot.PositionReason;
			PositionReasonInjestRaceId = leaderboardPilot.PositionReasonInjestRaceId;
			Flags = leaderboardPilot.Flags;
		}

		public bool Merge(InjestLeaderboardPilot leaderboardPilot)
		{
			bool changed = false;
			if (leaderboardPilot.InjestPilotId != null && !string.IsNullOrWhiteSpace(leaderboardPilot.InjestPilotId) && InjestPilotId != leaderboardPilot.InjestPilotId)
			{
				InjestPilotId = leaderboardPilot.InjestPilotId;
				changed = true;
			}
			if (leaderboardPilot.InjestPilotEntryId != null && !string.IsNullOrWhiteSpace(leaderboardPilot.InjestPilotEntryId) && InjestPilotEntryId != leaderboardPilot.InjestPilotEntryId)
			{
				InjestPilotEntryId = leaderboardPilot.InjestPilotEntryId;
				changed = true;
			}
			if (leaderboardPilot.InjestName != null && !string.IsNullOrWhiteSpace(leaderboardPilot.InjestName) && InjestName != leaderboardPilot.InjestName)
			{
				InjestName = leaderboardPilot.InjestName;
				changed = true;
			}
			if (leaderboardPilot.Position != null && Position != leaderboardPilot.Position)
			{
				Position = leaderboardPilot.Position;
				changed = true;
			}
			if (leaderboardPilot.PositionDelta != null && PositionDelta != leaderboardPilot.PositionDelta)
			{
				PositionDelta = leaderboardPilot.PositionDelta;
				changed = true;
			}
			if (leaderboardPilot.PositionReason != null && !string.IsNullOrWhiteSpace(leaderboardPilot.PositionReason) && PositionReason != leaderboardPilot.PositionReason)
			{
				PositionReason = leaderboardPilot.PositionReason;
				changed = true;
			}
			if (leaderboardPilot.PositionReasonInjestRaceId != null && PositionReasonInjestRaceId != leaderboardPilot.PositionReasonInjestRaceId)
			{
				PositionReasonInjestRaceId = leaderboardPilot.PositionReasonInjestRaceId;
				changed = true;
			}
			if (leaderboardPilot.Flags != null && Flags != leaderboardPilot.Flags)
			{
				Flags = leaderboardPilot.Flags;
				changed = true;
			}
			return changed;
		}
	}
}
