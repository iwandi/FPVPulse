namespace FPVPulse.LocalHost.Client
{
    public enum ChangeGroup
    {
        None,

        InjestEvent,
		InjestEventData,

		InjestRace,
		InjestRaceData,

        InjestRacePilot,
		InjestRacePilotData,

		InjestPilotResult,
		InjestPilotResultData,

		InjestPosition,
		InjestPositionData,
	}

    public class ChangeEventArgs<T> : EventArgs
    {
        public ChangeGroup Group { get; }
        public int Id { get; }
        public int ParentId { get; }
        public T Data { get; }

        public ChangeEventArgs(ChangeGroup group, int id, int parentId, T data)
        {
            Group = group;
            Id = id;
            ParentId = parentId;
			Data = data;
		}
    }

    public static class ChangeSignalMessages
    {
        public static string Subscribe = "Subscribe";
		public static string Unsubscribe = "Unsubscribe";

		public static string Change = "C";
		public static string ChangeInjestEventData = "CIED";
		public static string ChangeInjestRaceData = "CIRC";
		public static string ChangeInjestRacePilotData = "CIRPD";
		public static string ChangeInjestPoilotResultData = "CIPRD";
		public static string ChangeInjestPositionData = "CIPD";
	}
}
