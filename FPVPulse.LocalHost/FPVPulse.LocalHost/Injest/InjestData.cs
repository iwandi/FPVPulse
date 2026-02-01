using FPVPulse.Ingest;

namespace FPVPulse.LocalHost.Injest
{
    public class InjestData
    {
        public IEnumerable<string> GetEventIds()
        {
            yield break;
        }

        public InjestEvent? GetEvent(string injestEventId)
        {
            return null;
        }

        public IEnumerable<string> GetRaceIds()
        {
            yield break;
        }

        public InjestRace? GetRace(string injestRaceId)
        {
            return null;
        }

        public InjestPilotResult? GetPilotResult(string injestRaceId, string injestPilotId)
        {
            return null;
        }
    }
}
