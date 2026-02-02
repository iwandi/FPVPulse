using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;

namespace FPVPulse.LocalHost.Injest.Db
{
    public class DbInjestRacePilot : InjestRacePilot
    {
        [Key]
        public int RacePilotId { get; set; }
        [Required]
        public int RaceId { get; set; }

        [Required]
        public string InjestId { get; set; } = string.Empty;
        [Required]
        public string InjestRaceId { get; set; } = string.Empty;

        public DbInjestRacePilot()
        {

        }

        public DbInjestRacePilot(string injestId, DbInjestRace race, InjestRacePilot racePilot)
        {
            RaceId = race.RaceId;

            InjestId = injestId;
            InjestRaceId = race.InjestRaceId;

            InjestName = racePilot.InjestName;
            SeedPosition = racePilot.SeedPosition;
            StartPosition = racePilot.StartPosition;
            Position = racePilot.Position;
            Channel = racePilot.Channel;
        }

        public bool Merge(InjestRacePilot racePilot)
        {
            bool changed = false;
            if (racePilot.InjestName != null && InjestName != racePilot.InjestName)
            {
                InjestName = racePilot.InjestName;
                changed = true;
            }
            if(racePilot.SeedPosition != null && SeedPosition != racePilot.SeedPosition)
            {
                SeedPosition = racePilot.SeedPosition;
                changed = true;
            }
            if(racePilot.StartPosition != null && StartPosition != racePilot.StartPosition) {
                StartPosition = racePilot.StartPosition;
                changed = true;
            }
            if (racePilot.Position != null && Position != racePilot.Position)
            {
                Position = racePilot.Position;
                changed = true;
            }
            if (racePilot.Channel != null && Channel != racePilot.Channel)
            {
                Channel = racePilot.Channel;
                changed = true;
            }
            return changed;
        }
    }
}
