using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class Race
	{
		[Key]
		public int RaceId { get; set; }

		public int InjestRaceId { get; set; }

		[ForeignKey(nameof(Event))]
		public int EventId { get; set; }

		[MaxLength(30)]
		public string Name { get; set; } = string.Empty;
		public RaceType RaceType { get; set; }

		public RaceLayout RaceLayout { get; set; }

		public int FirstOrderPoistion { get; set; }
		public int SecondOrderPosition { get; set; }

		[ForeignKey("RaceId")]
		public RacePilot[]? Pilots { get; set; }

		[ForeignKey("RaceId")]
		public RacePilotResult[]? Results { get; set; }
	}
}
