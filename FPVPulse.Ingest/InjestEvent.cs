using Newtonsoft.Json;
using System;

namespace FPVPulse.Ingest
{
    public class InjestEvent
    {
        [JsonProperty(Required = Required.Always)]
        public string InjestEventId { get; set; } = String.Empty;

        public string? InjestName { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? CurrentInjestRaceId { get; set; }
        public string? NextInjestRaceId { get; set; }

		public float? CurrentRaceRunTimeSeconds { get; set; }

		public DateTime? NextRaceSheduledStartTime { get; set; }
		public float? NextRaceSheduledStartSeconds { get; set; }
	}
}
