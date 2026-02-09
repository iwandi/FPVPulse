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

		InjestLeaderboard,
		InjestLeaderboardData,

		InjestLeaderboardPilot,
		InjestLeaderboardPilotData,

		Event,
		EventData,

		EventShedule,
		EventSheduleData,

		Leaderboard,
		LeaderboardData,

		LeaderboardPilot,
		LeaderboardPilotData,

		Pilot,
		PilotData,

		Race,
		RaceData,

		RacePilot,
		RacePilotData,

		RacePilotResult,
		RacePilotResultData,
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
		public static string ChangeInjestPilotResultData = "CIPRD";
		public static string ChangeInjestLeaderboardData = "CILD";
		public static string ChangeInjestLeaderboardPilotData = "CILPD";

		public static string ChangeEventData = "CED";
		public static string ChangeEventSheduleData = "CESD";
		public static string ChangeLeaderboardData = "CLD";
		public static string ChangeLeaderboardPilotData = "CLPD";
		public static string ChangePilotData = "CPD";
		public static string ChangeRaceData = "CRD";
		public static string ChangeRacePilotData = "CRPD";
		public static string ChangeRacePilotResultData = "CRPRD";
	}
}
