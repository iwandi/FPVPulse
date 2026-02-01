namespace FPVPulse.Core.Data
{
    public class RacePilot
    {
        public Guid RacePilotId { get; set; }

        public Guid RaceId { get; set; }
        public Guid PilotId { get; set; }
        public Guid QualifyingRaceId { get; set; }

        public bool IsCheckedIn { get; set; }

        public uint StartPosition { get; set; }
        public uint QualifyingPosition { get; set; }

        public uint FinishingPosition { get; set; }

        public bool IsAdvaning { get; set; }
        public bool IsDead { get; set; }

        public uint FlasStartCount { get; set; }

        public RacePilotFlag Flags { get; set; }
    }

    public enum RacePilotFlag
    {
        None = 0,
        IronMan = 1,
    }
}
