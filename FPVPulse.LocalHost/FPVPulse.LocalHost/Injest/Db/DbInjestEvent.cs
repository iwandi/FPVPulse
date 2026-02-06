using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;

namespace FPVPulse.LocalHost.Injest.Db
{
    public class DbInjestEvent : InjestEvent
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public string InjestId { get; set; } = string.Empty;

        public DbInjestEvent()
        {

        }

        public DbInjestEvent(string injestId, InjestEvent @event)
        {
            InjestId = injestId;

            InjestEventId = @event.InjestEventId;
            InjestName = @event.InjestName;
            StartDate = @event.StartDate;
            EndDate = @event.EndDate;
            CurrentInjestRaceId = @event.CurrentInjestRaceId;
            NextInjestRaceId = @event.NextInjestRaceId;
            NextRaceSheduledStartTime = @event.NextRaceSheduledStartTime;
        }

        public bool Merge(InjestEvent @event)
        {
            bool changed = false;
            if (@event.InjestName != null && !string.IsNullOrWhiteSpace(@event.InjestName) && InjestName != @event.InjestName)
            {
                InjestName = @event.InjestName;
                changed = true;
            }
            if (@event.StartDate != null && StartDate != @event.StartDate)
            {
                StartDate = @event.StartDate;
                changed = true;
            }
            if (@event.EndDate != null && EndDate != @event.EndDate)
            {
                EndDate = @event.EndDate;
                changed = true;
            }
            if (@event.CurrentInjestRaceId != null && CurrentInjestRaceId != @event.CurrentInjestRaceId)
            {
                CurrentInjestRaceId = @event.CurrentInjestRaceId;
                changed = true;
            }
            if (@event.NextInjestRaceId != null && NextInjestRaceId != @event.NextInjestRaceId)
            {
                NextInjestRaceId = @event.NextInjestRaceId;
                changed = true;
            }
            if (CurrentRaceRunTimeSeconds != @event.CurrentRaceRunTimeSeconds)
            {
				CurrentRaceRunTimeSeconds = @event.CurrentRaceRunTimeSeconds;
                changed = true;
			}
			if (NextRaceSheduledStartTime != @event.NextRaceSheduledStartTime)
			{
				NextRaceSheduledStartTime = @event.NextRaceSheduledStartTime;
				changed = true;
			}
			if (NextRaceSheduledStartSeconds != @event.NextRaceSheduledStartSeconds)
			{
				NextRaceSheduledStartSeconds = @event.NextRaceSheduledStartSeconds;
				changed = true;
			}
			return changed;
        }
    }
}
