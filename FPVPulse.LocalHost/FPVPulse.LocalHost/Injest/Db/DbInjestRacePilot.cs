using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;

namespace FPVPulse.LocalHost.Injest.Db
{
    public class DbInjestRacePilot : InjestRacePilot
    {
        [Key]
        public int RacePilotId { get; set; }

        [Required]
        public string InjestId { get; set; } = string.Empty;
        [Required]
        public int RaceId { get; set; }

        public DbInjestRacePilot()
        {

        }

        public DbInjestRacePilot(string injestId, InjestRacePilot racePilot)
        {
            InjestId = injestId;

            InjestName = racePilot.InjestName;
            SeedPosition = racePilot.SeedPosition;
            StartPosition = racePilot.StartPosition;
            Position = racePilot.Position;
            Channel = racePilot.Channel;
        }

        public bool Merge(InjestRacePilot racePilot)
        {
            bool changed = false;
            if (racePilot.InjestName != null)
            {
                InjestName = racePilot.InjestName;
                changed = true;
            }
            if(racePilot.SeedPosition != null)
            {
                SeedPosition = racePilot.SeedPosition;
                changed = true;
            }
            if(racePilot.StartPosition != null) {
                StartPosition = racePilot.StartPosition;
                changed = true;
            }
            if (racePilot.Position != null)
            {
                Position = racePilot.Position;
                changed = true;
            }
            if (racePilot.Channel != null)
            {
                Channel = racePilot.Channel;
                changed = true;
            }
            return changed;
        }
    }
}
