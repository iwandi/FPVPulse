using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class Event
	{
		[Key]
		public int EventId { get; set; }

		public int InjestEventId { get; set; }

		[MaxLength(30)]
		public string Name { get; set; } = string.Empty;

		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		//[ForeignKey("EventId")]
		public EventShedule? Shedule { get; set; }

		[NotMapped]
		public Race[]? Races { get; set; }
	}
}
