using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class RacePilot
	{
		[Key]
		public int RacePilotId;

		public int InjestRacePilotId;

		[ForeignKey(nameof(RacePilot))]
		public int PilotId;

		public int? SeedPosition;
		public int? SeedRaceId;

		public int StartPosition;
		public int Position;

		[MaxLength(10)]
		public string Channel = string.Empty;

		[ForeignKey("RacePilotId")]
		public RacePilotResult? Result { get; set; }

		[ForeignKey("PilotId")]
		public Pilot? Pilot { get; set; }
	}
}
