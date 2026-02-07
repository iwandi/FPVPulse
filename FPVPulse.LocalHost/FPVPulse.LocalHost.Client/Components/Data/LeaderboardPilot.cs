using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class LeaderboardPilot
	{
		[Key]
		public int LeaderboardPilotId { get; set; }

		public int InjestLeaderboardPilotId { get; set; }

		[ForeignKey(nameof(Leaderboard))]
		public int LeaderboardId { get; set; }

		[ForeignKey(nameof(Pilot))]
		public int PilotId { get; set; }

		public int? Position { get; set; }
		public int? PositionDelta { get; set; }

		[MaxLength(30)]
		public string? PositionReason { get; set; }
		[ForeignKey(nameof(Race))]
		public int? PositionReasonRaceId { get; set; }

		public LeaderboardPilotFlag? Flags { get; set; }
	}
}
