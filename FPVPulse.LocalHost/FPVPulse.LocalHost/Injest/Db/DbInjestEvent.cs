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
            if (@event.InjestName != null)
            {
                InjestName = @event.InjestName;
                changed = true;
            }
            if (@event.StartDate != null)
            {
                StartDate = @event.StartDate;
                changed = true;
            }
            if (@event.EndDate != null)
            {
                EndDate = @event.EndDate;
                changed = true;
            }
            if (@event.CurrentInjestRaceId != null)
            {
                CurrentInjestRaceId = @event.CurrentInjestRaceId;
                changed = true;
            }
            if (@event.NextInjestRaceId != null)
            {
                NextInjestRaceId = @event.NextInjestRaceId;
                changed = true;
            }
            if (@event.NextRaceSheduledStartTime != null)
            {
                NextRaceSheduledStartTime = @event.NextRaceSheduledStartTime;
                changed = true;
            }
            return changed;
        }
    }
}
