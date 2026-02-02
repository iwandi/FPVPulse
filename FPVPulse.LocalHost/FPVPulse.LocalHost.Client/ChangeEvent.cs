namespace FPVPulse.LocalHost.Client
{
    public enum ChangeGroup
    {
        None,
        InjestEvent,
        InjestRace,
        InjestPilotResult
    }

    public class ChangeEventArgs : EventArgs
    {
        public ChangeGroup Group { get; }
        public int Id { get; }
        public int ParentId { get; }

        public ChangeEventArgs(ChangeGroup group, int id, int parentId)
        {
            Group = group;
            Id = id;
            ParentId = parentId;
        }
    }

    public static class ChangeSignalMessages
    {
        public static string Change = "C";
    }
}
