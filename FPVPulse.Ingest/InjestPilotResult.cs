using Newtonsoft.Json;
using System;

namespace FPVPulse.Ingest
{
    public class InjestPilotResult
    {
        [JsonProperty(Required = Required.Always)]
        public string InjestRaceId { get; set; } = String.Empty;
        [JsonProperty(Required = Required.Always)]
        public string InjestPilotId { get; set; } = String.Empty;

        public int? CurrentSector { get; set; }
        public int? CurrentSplit { get; set; }

        public int? Position { get; set; }

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

        public InjestLap[]? Laps { get; set; }
    }

    public enum ResultFlag
    {
        None = 0,
        DidNotStart = 1,
        DidNotFinish = 2,
        Disqualified = 3,
        Invalid = 4,
        FalseStart = 5,
    }

    public class InjestLap
    {
        public int LapNumber { get; set; }
        public float? LapTime { get; set; }

        public bool? IsInvalid { get; set; }
    }
}
