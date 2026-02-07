using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class Leaderboard
	{
		[Key]
		public int LeaderboardId { get; set; }

		public int InjestLeaderboardId { get; set; }

		[ForeignKey(nameof(Event))]
		public int EventId { get; set; }
		public int RaceType { get; set; }

		[ForeignKey("LeaderboardId")]
		public LeaderboardPilot[]? Pilots { get; set; }
	}
}
