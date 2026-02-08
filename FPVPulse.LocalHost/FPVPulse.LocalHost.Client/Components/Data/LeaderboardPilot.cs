using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class LeaderboardPilot
	{
		[Key]
		public int LeaderboardPilotId;

		public int InjestLeaderboardPilotId;

		[ForeignKey(nameof(Leaderboard))]
		public int LeaderboardId;

		[ForeignKey(nameof(Pilot))]
		public int PilotId;

		public int? Position;
		public int? PositionDelta;

		[MaxLength(30)]
		public string? PositionReason;
		[ForeignKey(nameof(Race))]
		public int? PositionReasonRaceId;

		public LeaderboardPilotFlag? Flags;
	}
}
