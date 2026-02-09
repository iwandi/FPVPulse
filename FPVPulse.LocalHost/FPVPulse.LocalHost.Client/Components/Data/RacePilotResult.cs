using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class RacePilotResult
	{
		[Key]
		public int RacePilotResultId { get; set; }

		public int InjestPilotResultId { get; set; }

		public int? LazyRaceId { get; set; }
		public int? LazyRacePilotId { get; set; }

		public int? Position { get; set; }

		public int? CurrentSector { get; set; }
		public int? CurrentSplit { get; set; }

		public DateTime? StartTime { get; set; }
		public DateTime? FinishTime { get; set; }

		public int? LapCount { get; set; }
		public float? TotalTime { get; set; }

		public float? TopLapTime { get; set; }
		public float? Top2ConsecutiveLapTime { get; set; }
		public float? Top3ConsecutiveLapTime { get; set; }

		public float? AverageLapTime { get; set; }

		public bool? IsComplited { get; set; }

		public ResultFlag? Flags { get; set; }

		public Lap[]? Laps { get; set; }
	}

	public class Lap
	{
		public int LapNumber;
		public float? LapTime;

		public bool? IsInvalid;
	}
}
