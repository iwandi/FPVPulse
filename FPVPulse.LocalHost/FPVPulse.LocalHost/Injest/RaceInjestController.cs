using Microsoft.AspNetCore.Mvc;
using FPVPulse.Ingest;

namespace FPVPulse.LocalHost.Injest
{
    [Route("api/injest/race")]
    [ApiController]
    public class RaceInjestController : ControllerBase
    {
        readonly InjestQueue queue;
        readonly InjestData data;

        public RaceInjestController(InjestQueue queue, InjestData data)
        {
            this.queue = queue;
            this.data = data;
        }

        bool TryGetInjestId(out string injestId)
        {
            injestId = Request.Headers["Injest-ID"].FirstOrDefault() ?? "";
            return !string.IsNullOrWhiteSpace(injestId);
        }

        string GetInjestId()
        {
            string? injestId = Request.Headers["Injest-ID"].FirstOrDefault();
            if (injestId == null)
                injestId = "";
            return injestId;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            if (TryGetInjestId(out var injestId))
            {
                return data.GetRaceIds(injestId);
            }
            return data.GetRaceIds();
        }

        [HttpGet("{injestRaceId}")]
        public ActionResult<InjestRace> Get(string injestRaceId)
        {
            if (string.IsNullOrWhiteSpace(injestRaceId))
                return BadRequest("Missing injestRaceId.");

            var race = data.GetRace(GetInjestId(), injestRaceId);
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

            queue.Enqueue(GetInjestId(), race);
            return Ok();
        }
    }
}
