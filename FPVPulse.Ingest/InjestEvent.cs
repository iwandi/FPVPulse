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

        public string? currentInjestRaceId { get; set; }
        public string? nextInjestRaceId { get; set; }

        public DateTime? NextRaceSheduledStartTime { get; set; }
    }
}
