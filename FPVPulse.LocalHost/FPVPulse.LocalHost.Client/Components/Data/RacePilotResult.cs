using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class RacePilotResult
	{
		[Key]
		public int RacePilotResultId;

		public int InjestPilotResultId;

		[ForeignKey(nameof(RacePilot))]
		public int RacePilotId;

		public int? Position;

		public int? CurrentSector;
		public int? CurrentSplit;

		public DateTime? StartTime;
		public DateTime? FinishTime;

		public int? LapCount;
		public float? TotalTime;

		public float? TopLapTime;
		public float? Top2ConsecutiveLapTime;
		public float? Top3ConsecutiveLapTime;

		public float? AverageLapTime;

		public bool? IsComplited;

		public ResultFlag? Flags;

		public Lap[]? Laps;
	}

	public class Lap
	{
		public int LapNumber;
		public float? LapTime;

		public bool? IsInvalid;
	}
}
