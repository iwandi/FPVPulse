using Microsoft.AspNetCore.Mvc;
using FPVPulse.Ingest;
using FPVPulse.LocalHost.Injest.Db;

namespace FPVPulse.LocalHost.Injest
{
    [Route("api/injest/inspect")]
    [ApiController]
    public class InjestInspectController : ControllerBase
    {
        readonly IServiceProvider serviceProvider;

        public InjestInspectController(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [HttpGet("event/")]
        public IEnumerable<int> GetEvents()
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.Events.Select(e => e.EventId).ToList();
        }

        [HttpGet("event/{eventId}")]
        public ActionResult<InjestEvent> GetEvent(int eventId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            var @event = db.Events.Find(eventId);

            if(@event == null)
                return NotFound();
            return @event;
        }

        [HttpGet("race/")]
        public IEnumerable<int> GetRaces()
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            return db.Races.Select(e => e.RaceId).ToList();
        }

        [HttpGet("race/{raceId}")]
        public ActionResult<InjestRace> GetRace(int raceId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            var race = db.Races.Find(raceId);

            if (race == null)
                return NotFound();

            InjestData.FillPilots(db, race);
            return race;
        }

        [HttpGet("race/{raceId}/result")]
        public IEnumerable<InjestPilotResult> GetPilotResults(int raceId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            var race = db.Races.Find(raceId);

            if (race == null)
                return null;

            return db.PilotResults.Where(r => r.InjestRaceId == race.InjestRaceId).ToArray();
        }

        [HttpGet("race/{raceId}/result/{resultId}")]
        public ActionResult<InjestPilotResult> GetPilotResult(int raceId, int resultId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            var pilotResult = db.PilotResults.Find(resultId);
            if (pilotResult == null)
                return NotFound();

            var race = db.Races.Find(resultId);
            if(race == null || race.InjestRaceId != pilotResult.InjestRaceId)
                return NotFound();

            return pilotResult;
        }

        [HttpGet("race/result/{resultId}")]
        public ActionResult<InjestPilotResult> GetPilotResult(int resultId)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            var pilotResult = db.PilotResults.Find(resultId);
            if (pilotResult == null)
                return NotFound();
            return pilotResult;
        }
    }
}
