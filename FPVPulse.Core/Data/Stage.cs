namespace FPVPulse.Core.Data
{
    public class Stage
    {
        public Guid StageId { get; set; }

        public Guid CompetitionId { get; set; }

        public Guid RuleSetId { get; set; }
        public StageType StageType { get; set; }
        public RaceMode RaceMode { get; set; }
    }

    public enum StageType
    {
        Practice = 0,
        Qualifying = 1,
        Final = 2,
    }

    public enum RaceMode
    {
        Continues = 0,
        SheduledRaces = 1,
        AdHocRaces = 2
    }
}
