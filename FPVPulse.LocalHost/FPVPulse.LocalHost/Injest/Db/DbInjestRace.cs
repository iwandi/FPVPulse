using FPVPulse.Ingest;
using System.ComponentModel.DataAnnotations;

namespace FPVPulse.LocalHost.Injest.Db
{
    public class DbInjestRace : InjestRace
    {
        [Key]
        public int RaceId { get; set; }

        [Required]
        public string InjestId { get; set; } = string.Empty;

        public DbInjestRace()
        {

        }

        public DbInjestRace(string injestId, InjestRace race)
        {
            InjestId = injestId;

            InjestRaceId = race.InjestRaceId;
            InjestEventId = race.InjestEventId;
            InjestName = race.InjestName;
            RaceType = race.RaceType;
            RaceLayout = race.RaceLayout;
            FirstOrderPoistion = race.FirstOrderPoistion;
            SecondOrderPosition = race.SecondOrderPosition;

            Pilots = race.Pilots;
        }

        public bool Merge(InjestRace race)
        {
            bool changed = false;
            if (race.InjestName != null && InjestName != race.InjestName)
            {
                InjestName = race.InjestName;
                changed = true;
            }
            if(race.RaceType != null && RaceType != race.RaceType)
            {
                RaceType = race.RaceType;
                changed = true;
            }
            if(race.RaceLayout != null && RaceLayout != race.RaceLayout)
            {
                RaceLayout = race.RaceLayout;
                changed = true;
            }
            if (race.FirstOrderPoistion != null && FirstOrderPoistion != race.FirstOrderPoistion)
            {
                FirstOrderPoistion = race.FirstOrderPoistion;
                changed = true;
            }
            if (race.SecondOrderPosition != null && SecondOrderPosition != race.SecondOrderPosition)
            {
                SecondOrderPosition = race.SecondOrderPosition;
                changed = true;
            }
            return changed;
        }
    }
}
