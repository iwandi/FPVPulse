using FPVPulse.Ingest;
using FPVPulse.LocalHost.Injest.Db;

namespace FPVPulse.LocalHost.Injest
{
    public class InjestData
    {
        readonly InjestDbContext db;

        public InjestData(InjestDbContext db)
        {
            this.db = db;
        }

        public IEnumerable<string> GetEventIds()
        {
            yield break;
        }

        public IEnumerable<string> GetEventIds(string injestId)
        {
            yield break;
        }

        public InjestEvent? GetEvent(int eventId)
        {
            return null;
        }

        public InjestEvent? GetEvent(string injestId, string injestEventId)
        {
            return null;
        }

        public IEnumerable<string> GetRaceIds()
        {
            yield break;
        }

        public IEnumerable<string> GetRaceIds(string injestId)
        {
            yield break;
        }

        public InjestRace? GetRace(int raceId)
        {
            return null;
        }

        public InjestRace? GetRace(string injestId, string injestRaceId)
        {
            return null;
        }

        public InjestPilotResult? GetPilotResult(int pilotResultId)
        {
            return null;
        }

        public InjestPilotResult? GetPilotResult(string injestId, string injestRaceId, string injestPilotId)
        {
            return null;
        }
    }
}
