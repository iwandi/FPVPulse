namespace FPVPulse.Core.Data
{
    public class Race
    {
        public Guid RaceId { get; set; }

        public Guid ReRunRaceId { get; set; }

        public Guid StageId { get; set; }

        public string Name { get; set; } = string.Empty;
        public DateTime ScheduledStartTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsInvalid { get; set; }
    }
}
