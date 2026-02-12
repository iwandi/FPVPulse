using FPVPulse.Ingest;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public static class DataUtil
	{
		public static string GetPilotName(LeaderboardPilot pilot)
		{
			if (pilot.Pilot != null)
				return pilot.Pilot.DisplayName;
			else
				return $"Pilot :{pilot.PilotId}";
		}

		public static string GetPilotNation(LeaderboardPilot pilot)
		{
			if (pilot.Pilot != null)
				return pilot.Pilot.CountryAlpha3;
			return string.Empty;
		}

		public static string GetPilotPosition(LeaderboardPilot pilot)
		{
			if (pilot.PositionDelta.HasValue && pilot.PositionDelta.Value != 0)
			{
				var delta = pilot.PositionDelta.Value;
				var deltaString = delta > 0 ? $"+{delta}" : delta.ToString();
				return $"{pilot.Position} ({deltaString})";
			}
			return pilot.Position.ToString();
		}

		public static string GetTime(Race? race, RacePilot? racePilot)
		{
			if (race != null && race.Results != null && racePilot != null)
			{
				var result = race.Results.Where(e => e.LazyRacePilotId == racePilot.RacePilotId).FirstOrDefault();
				return GetTime(result);
			}
			return string.Empty;
		}

		public static string GetTime(RacePilotResult? result)
		{
			if (result != null)
			{
				if (result.Top3ConsecutiveLapTime.HasValue)
					return $"(3) {result.Top3ConsecutiveLapTime}";
				if (result.Top2ConsecutiveLapTime.HasValue)
					return $"(2) {result.Top2ConsecutiveLapTime}";
				if (result.TopLapTime.HasValue)
					return $"(1) {result.TopLapTime}";
			}
			return string.Empty;
		}

		public static string PrintFlags(RacePilotResult? result)
		{
			if (result == null || result.Flags == null || result.Flags == ResultFlag.None)
			{
				return string.Empty;
			}
			return result.Flags.ToString();
		}

		public static string PrintLap(RacePilotResult? result, int lapIndex)
		{
			if (result == null || result.Laps == null || lapIndex >= result.Laps.Length)
			{
				return string.Empty;
			}
			return result.Laps[lapIndex].LapTime.ToString();
		}
	}
}
