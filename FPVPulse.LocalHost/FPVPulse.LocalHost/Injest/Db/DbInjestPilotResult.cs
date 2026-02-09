using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;

namespace FPVPulse.LocalHost.Injest.Db
{
    public class DbInjestPilotResult : InjestPilotResult
    {
        [Key]
        public int PilotResultId { get; set; }
		
		// We allow a non assoiated PilotResult as we can map it later back
		public int? RaceId { get; set; }
		public int? RacePilotId { get; set; }
		[Required]
        public string InjestId { get; set; } = string.Empty;

        public DbInjestPilotResult()
        {

        }

        public DbInjestPilotResult(string injestId, InjestPilotResult pilotResult, DbInjestRace? race, DbInjestRacePilot? pilot)
        {
            InjestId = injestId;
            if (race != null)
                RaceId = race.RaceId;
            if(pilot != null)
				RacePilotId = pilot.RacePilotId;

			InjestRaceId = pilotResult.InjestRaceId;
            InjestPilotId = pilotResult.InjestPilotId;
            InjestPilotEntryId = pilotResult.InjestPilotEntryId;

            CurrentSector = pilotResult.CurrentSector;
            CurrentSplit = pilotResult.CurrentSplit;
            Position = pilotResult.Position;
            StartTime = pilotResult.StartTime;
            FinishTime = pilotResult.FinishTime;
            LapCount = pilotResult.LapCount;
            TotalTime = pilotResult.TotalTime;
            TopLapTime = pilotResult.TopLapTime;
            Top2ConsecutiveLapTime = pilotResult.Top2ConsecutiveLapTime;
            Top3ConsecutiveLapTime = pilotResult.Top3ConsecutiveLapTime;
            AverageLapTime = pilotResult.AverageLapTime;
            IsComplited = pilotResult.IsComplited;
            Flags = pilotResult.Flags;
            Laps = pilotResult.Laps;
        }

        public bool Merge(InjestPilotResult pilotResult, DbInjestRace? race)
        {
            bool changed = false;
			if (pilotResult.InjestPilotId != null && !string.IsNullOrWhiteSpace(pilotResult.InjestPilotId) && InjestPilotId != pilotResult.InjestPilotId)
			{
				InjestPilotId = pilotResult.InjestPilotId;
				changed = true;
			}
			if (pilotResult.InjestPilotEntryId != null && !string.IsNullOrWhiteSpace(pilotResult.InjestPilotEntryId) && InjestPilotEntryId != pilotResult.InjestPilotEntryId)
			{
				InjestPilotEntryId = pilotResult.InjestPilotEntryId;
				changed = true;
			}
			if (pilotResult.CurrentSector != null && CurrentSector != pilotResult.CurrentSector)
            {
                CurrentSector = pilotResult.CurrentSector;
                changed = true;
            }
            if (pilotResult.CurrentSplit != null && CurrentSplit != pilotResult.CurrentSplit)
            {
                CurrentSplit = pilotResult.CurrentSplit;
                changed = true;
            }
            if (pilotResult.Position != null && Position != pilotResult.Position)
            {
                Position = pilotResult.Position;
                changed = true;
            }
            if (pilotResult.StartTime != null && StartTime != pilotResult.StartTime)
            {
                StartTime = pilotResult.StartTime;
                changed = true;
            }
            if (pilotResult.FinishTime != null && FinishTime != pilotResult.FinishTime)
            {
                FinishTime = pilotResult.FinishTime;
                changed = true;
            }
            if (pilotResult.LapCount != null && LapCount != pilotResult.LapCount)
            {
                LapCount = pilotResult.LapCount;
                changed = true;
            }
            if (pilotResult.TotalTime != null && TotalTime != pilotResult.TotalTime)
            {
                TotalTime = pilotResult.TotalTime;
                changed = true;
            }
            if (pilotResult.TopLapTime != null && TopLapTime != pilotResult.TopLapTime)
            {
                TopLapTime = pilotResult.TopLapTime;
                changed = true;
            }
            if (pilotResult.Top2ConsecutiveLapTime != null && Top2ConsecutiveLapTime != pilotResult.Top2ConsecutiveLapTime)
            {
                Top2ConsecutiveLapTime = pilotResult.Top2ConsecutiveLapTime;
                changed = true;
            }
            if (pilotResult.Top3ConsecutiveLapTime != null && Top3ConsecutiveLapTime != pilotResult.Top3ConsecutiveLapTime)
            {
                Top3ConsecutiveLapTime = pilotResult.Top3ConsecutiveLapTime;
                changed = true;
            }
            if (pilotResult.AverageLapTime != null && AverageLapTime != pilotResult.AverageLapTime)
            {
                AverageLapTime = pilotResult.AverageLapTime;
                changed = true;
            }
            if (pilotResult.IsComplited != null && IsComplited != pilotResult.IsComplited)
            {
                IsComplited = pilotResult.IsComplited;
                changed = true;
            }
            if (pilotResult.Flags != null && Flags != pilotResult.Flags)
            {
                Flags = pilotResult.Flags;
                changed = true;
            }
            if (pilotResult.Laps != null && !CheckLaps(Laps, pilotResult.Laps))
            {
                Laps = pilotResult.Laps;
                changed = true;
            }

            if (race != null)
            {
                if (RaceId != race.RaceId)
                {
                    RaceId = race.RaceId;
                    changed = true;
                }
                if (race.Pilots != null)
                {
                    if (InjestPilotId == null && InjestPilotEntryId != null)
                    {
                        var racePilot = race.Pilots.FirstOrDefault(p => p.InjestPilotEntryId == InjestPilotEntryId);
                        if(racePilot != null && racePilot.InjestPilotId != null && !string.IsNullOrWhiteSpace(racePilot.InjestPilotId))
                        {
                            InjestPilotId = racePilot.InjestPilotId;
							changed = true;
						}
					}
                    else if (InjestPilotId != null && InjestPilotEntryId == null)
                    {
						var racePilot = race.Pilots.FirstOrDefault(p => p.InjestPilotId == InjestPilotId);
						if (racePilot != null && racePilot.InjestPilotEntryId != null && !string.IsNullOrWhiteSpace(racePilot.InjestPilotEntryId))
						{
							InjestPilotEntryId = racePilot.InjestPilotEntryId;
							changed = true;
						}
					}
                }
			}
            return changed;
        }

        bool CheckLaps(InjestLap[]? laps1, InjestLap[]? laps2)
		{
			if (laps1 == null && laps2 == null)
				return true;
			if (laps1 == null || laps2 == null)
				return false;
			if (laps1.Length != laps2.Length)
				return false;
			for (int i = 0; i < laps1.Length; i++)
			{
				var lap1 = laps1[i];
				var lap2 = laps2[i];
				if (lap1.LapNumber != lap2.LapNumber)
					return false;
				if (lap1.LapTime != lap2.LapTime)
					return false;
				if (lap1.IsInvalid != lap2.IsInvalid)
					return false;
			}
			return true;
		}
	}
}
