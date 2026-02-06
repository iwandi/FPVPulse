using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPVPulse.Ingest
{
	public enum LeaderboardPilotFlag
	{
		Unknown = 0,
		InCurrentRace = 1,
		InNextRace = 2,
		Disqalified = 3,
		FinalPosition = 4,
	}

	public class InjestLeaderboard
	{
		public string? InjestEventId { get; set; }
		public string? InjestLeaderboardId { get; set; }
		public RaceType? RaceType { get; set; }

		public InjestLeaderboardPilot[]? Results { get; set; }
	}

	public class InjestLeaderboardPilot
	{
		public string InjestPilotId { get; set; } = String.Empty;
		public string InjestPilotEntryId { get; set; } = String.Empty;

		public string? InjestName { get; set; }

		public int? Position { get; set; }
		public int? PositionDelta { get; set; }

		public string? PositionReason { get; set; }
		public string? PositionReasonInjestRaceId { get; set; }

		public LeaderboardPilotFlag? Flags { get; set; }
	}
}
