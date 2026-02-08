using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class EventShedule
	{
		[Key]
		public int EventSheduleId;

		public int InjestEventId;

		[ForeignKey(nameof(Event))]
		public int EventId;

		public int? CurrentRaceId;
		public int? NextRaceId;

		public TimeSpan? RaceTimeLimit;

		public DateTime? CurrentRaceStartTime;
		public DateTime? NextRaceStartTime;
	}
}
