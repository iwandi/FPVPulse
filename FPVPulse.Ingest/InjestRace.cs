using Newtonsoft.Json;
using System;

namespace FPVPulse.Ingest
{
    public enum RaceType
    {
        Unknown = 0,
        Qualifying = 1,
        Mains = 2,
        Practice = 3
    }

    public enum RaceLayout
    {
        Unknown = 0,
        Continues = 1,
        SheduledHeats = 2,
        AdHoc = 3,
        Ladder = 4, // How to id A/B Ladder with merge Finals ?
        Tree = 5,
    }

    public class InjestRace
    {
        [JsonProperty(Required = Required.Always)]
        public string InjestRaceId { get; set; } = String.Empty;

        public string? InjestEventId { get; set; }

        public string? InjestName { get; set; }
        public RaceType? RaceType { get; set; }

        public RaceLayout? RaceLayout { get; set; }
        public int? FirstOrderPoistion { get; set; }
        public int? SecondOrderPosition { get; set; }

        public InjestRacePilot[]? Pilots { get; set; }

        public static int GetRaceTypeOrder(RaceType? raceType)
        {
            if(raceType == null)
                return int.MaxValue;
            switch (raceType)
            {
                case Ingest.RaceType.Practice:
                    return 1;
                case Ingest.RaceType.Qualifying:
                    return 2;
                case Ingest.RaceType.Mains:
                    return 3;
                default:
                    return (int)raceType;
            }
        }
    }

    public class InjestRacePilot
    {
        [JsonProperty(Required = Required.Always)]
        public string InjestPilotId { get; set; } = String.Empty;
		public string InjestPilotEntryId { get; set; } = String.Empty;

		public string? InjestName { get; set; }

        public int? SeedPosition { get; set; }
        public int? StartPosition { get; set; }
        public int? Position { get; set; }

        public string? Channel { get; set; }
    }
}
