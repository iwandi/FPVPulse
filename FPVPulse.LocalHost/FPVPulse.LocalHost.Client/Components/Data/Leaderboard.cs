using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class Leaderboard
	{
		[Key]
		public int LeaderboardId;

		public int InjestLeaderboardId;

		[ForeignKey(nameof(Event))]
		public int EventId;
		public RaceType RaceType;

		[ForeignKey("LeaderboardId")]
		public LeaderboardPilot[]? Pilots { get; set; }
	}
}
