using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class Event
	{
		[Key]
		public int EventId;

		public int InjestEventId;

		[MaxLength(30)]
		public string Name = string.Empty;

		public DateTime? StartDate;
		public DateTime? EndDate;

		[ForeignKey("EventId")]
		public EventShedule? Shedule { get; set; }

		[ForeignKey("EventId")]
		public Race[]? Races { get; set; }
	}
}
