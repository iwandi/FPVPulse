using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class EventShedule
	{
		[Key]
		public int EventSheduleId { get; set; }

		public int InjestEventId { get; set; }

		[ForeignKey(nameof(Event))]
		public int EventId { get; set; }

		public int? CurrentRaceId { get; set; }
		public int? NextRaceId { get; set; }

		public TimeSpan? RaceTimeLimit { get; set; }

		public DateTime? CurrentRaceStartTime { get; set; }
		public DateTime? NextRaceStartTime { get; set; }
	}
}
