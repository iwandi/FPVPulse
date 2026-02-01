using Microsoft.AspNetCore.Mvc;
using FPVPulse.Ingest;

namespace FPVPulse.LocalHost.Injest
{
    [Route("api/injest/race")]
    [ApiController]
    public class RaceInjestController : ControllerBase
    {
        private readonly InjestQueue queue;
        private readonly InjestData data;

        public RaceInjestController(InjestQueue queue, InjestData data)
        {
            this.queue = queue;
            this.data = data;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return data.GetRaceIds();
        }

        [HttpGet("{injestRaceId}")]
        public ActionResult<InjestRace> Get(string injestRaceId)
        {
            if (string.IsNullOrWhiteSpace(injestRaceId))
                return BadRequest("Missing injestRaceId.");

            var race = data.GetRace(injestRaceId);
            if (race == null)
                return NotFound();

            return race;
        }

        // TODO : how to we structure the id mapping from the Injest ids to the acctual ids ?
        [HttpPut("{injestRaceId}")]
        public async Task<IActionResult> Put(string injestRaceId, [FromBody] InjestRace race)
        {
            if(string.IsNullOrWhiteSpace(injestRaceId))
                return BadRequest("Missing injestRaceId.");

            if(race == null)
                return BadRequest("Failed to deserialize InjestRace = null");

            if(race.InjestRaceId != injestRaceId)
                return BadRequest($"injestRaceId missamch {injestRaceId} != {race.InjestRaceId}");

            queue.Enqueue(race);
            return Ok();
        }
    }
}
