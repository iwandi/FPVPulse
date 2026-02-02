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
        [Required]
        public string InjestId { get; set; } = string.Empty;

        public DbInjestPilotResult()
        {

        }

        public DbInjestPilotResult(string injestId, InjestPilotResult pilotResult, DbInjestRace? race)
        {
            InjestId = injestId;
            if (race != null)
                RaceId = race.RaceId;

            InjestRaceId = pilotResult.InjestRaceId;
            InjestPilotId = pilotResult.InjestPilotId;

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
            if (pilotResult.Laps != null && Laps != pilotResult.Laps)
            {
                Laps = pilotResult.Laps;
                changed = true;
            }

            if(race != null && RaceId != race.RaceId)
            {
                RaceId = race.RaceId;
                changed = true;
            }
            return changed;
        }
    }
}
