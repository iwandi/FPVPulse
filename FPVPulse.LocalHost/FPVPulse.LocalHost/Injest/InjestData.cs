using FPVPulse.Ingest;
using FPVPulse.LocalHost.Injest.Db;

namespace FPVPulse.LocalHost.Injest
{
    public class InjestData
    {
        readonly IServiceProvider serviceProvider;

        public InjestData(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<string> GetEventIds()
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.Events.Select(e => e.EventId.ToString()).Distinct().ToList();
        }

        public IEnumerable<string> GetEventIds(string injestId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.Events.Where(e => e.InjestId == injestId)
                .Select(e => e.InjestEventId).ToList();
        }

        public InjestEvent? GetEvent(int eventId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.Events.Find(eventId);
        }

        public InjestEvent? GetEvent(string injestId, string injestEventId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.Events.FirstOrDefault(e => e.InjestId == injestId &&
                e.InjestEventId == injestEventId);
        }

        public IEnumerable<string> GetRaceIds()
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.Races.Select(r => r.RaceId.ToString()).Distinct().ToList();
        }

        public IEnumerable<string> GetRaceIds(string injestId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.Races.Where(r => r.InjestId == injestId)
                .Select(r => r.InjestRaceId).ToList();
        }

        public InjestRace? GetRace(int raceId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.Races.Find(raceId);
        }

        public InjestRace? GetRace(string injestId, string injestRaceId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.Races.FirstOrDefault(r => r.InjestId == injestId &&
                r.InjestRaceId == injestRaceId);
        }

        public InjestPilotResult? GetPilotResult(int pilotResultId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.PilotResults.Find(pilotResultId);
        }

        public InjestPilotResult? GetPilotResult(string injestId, string injestRaceId, string injestPilotId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.PilotResults.FirstOrDefault(pr => pr.InjestId == injestId &&
                pr.InjestRaceId == injestRaceId &&
                pr.InjestPilotId == injestPilotId);
        }
    }
}
