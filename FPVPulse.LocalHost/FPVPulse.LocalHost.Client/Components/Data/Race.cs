using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class Race
	{
		[Key]
		public int RaceId;

		public int InjestRaceId;

		[ForeignKey(nameof(Event))]
		public int EventId;

		[MaxLength(30)]
		public string Name = string.Empty;
		public RaceType RaceType;

		public RaceLayout RaceLayout;

		public int FirstOrderPoistion;
		public int SecondOrderPosition;

		[ForeignKey("RaceId")]
		public RacePilot[]? Pilots { get; set; }

		[ForeignKey("RaceId")]
		public RacePilotResult[]? Results { get; set; }
	}
}
