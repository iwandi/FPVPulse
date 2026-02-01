namespace FPVPulse.Core.Data
{
    public class RuleSet
    {
        public Guid RuleSetId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public uint PilotsPerRace { get; set; }
        public uint BumpUpCount { get; set; }
        public uint Lives { get; set; }
        public uint BestOfCount { get; set; }

        public RuleSetShape Shape { get; set; }
        public RuleSetFlag Flags { get; set; }
    }

    public enum  RuleSetShape
    {
        None = 0,
        Ladder = 1,
        Tree = 2,
    }

    public enum RuleSetFlag
    {
        None = 0,
        IronManMode = 1,
    }
}
