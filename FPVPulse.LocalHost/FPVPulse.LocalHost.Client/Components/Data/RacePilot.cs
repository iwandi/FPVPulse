using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class RacePilot
	{
		[Key]
		public int RacePilotId { get; set; }

		[ForeignKey(nameof(Race))]
		public int RaceId { get; set; }

		[ForeignKey(nameof(Event))]
		public int EventId { get; set; }

		public int InjestRacePilotId { get; set; }

		[ForeignKey(nameof(RacePilot))]
		public int PilotId { get; set; }

		public int? SeedPosition { get; set; }
		public int? SeedRaceId { get; set; }

		public int StartPosition { get; set; }
		public int Position { get; set; }

		[MaxLength(10)]
		public string Channel { get; set; } = string.Empty;

		//[ForeignKey("RacePilotId")]
		public RacePilotResult? Result { get; set; }

		//[ForeignKey("PilotId")]
		public Pilot? Pilot { get; set; }
	}
}
