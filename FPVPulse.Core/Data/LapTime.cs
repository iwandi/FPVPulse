namespace FPVPulse.Core.Data
{
    public class LapTime
    {
        public Guid LapTimeId { get; set; }
        public Guid RacePilotId { get; set; }

        public uint LapNumber { get; set; }
        public TimeSpan Time { get; set; }
        public uint Position { get; set; }

        public TimeSpan DeltaAhead { get; set; }
        public TimeSpan DeltaBehind { get; set; }

        public LapTimeFlag Flags { get; set; }
    }

    public enum LapTimeFlag
    {
        None = 0,
        FastestRaceLap = 1,
        FastestStageLap = 2,
        FastestCompetitionLap = 4,
    }
}
