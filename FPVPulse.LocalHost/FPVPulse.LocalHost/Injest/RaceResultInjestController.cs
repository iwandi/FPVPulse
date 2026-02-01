using FPVPulse.Ingest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FPVPulse.LocalHost.Injest
{
    [Route("api/injest/race")]
    [ApiController]
    public class PilotResultInjestController : Controller
    {
        private readonly InjestQueue queue;
        private readonly InjestData data;

        public PilotResultInjestController(InjestQueue queue, InjestData data)
        {
            this.queue = queue;
            this.data = data;
        }

        [HttpGet("{injestRaceId}/result/{injestPilotId}")]
        public ActionResult<InjestPilotResult> Get(string injestRaceId, string injestPilotId)
        {
            if (string.IsNullOrWhiteSpace(injestRaceId))
                return BadRequest("Missing injestRaceId.");

            if (string.IsNullOrWhiteSpace(injestPilotId))
                return BadRequest("Missing injestPilotId.");

            var result = data.GetPilotResult(injestRaceId, injestPilotId);
            if (result == null)
                return NotFound();

            return result;
        }

        [HttpPut("{injestRaceId}/result/{injestPilotId}")]
        public async Task<IActionResult> Put(string injestRaceId, string injestPilotId, [FromBody] InjestPilotResult result)
        {
            if (string.IsNullOrWhiteSpace(injestRaceId))
                return BadRequest("Missing injestRaceId.");
            if (string.IsNullOrWhiteSpace(injestPilotId))
                return BadRequest("Missing injestPilotId.");

            if (result == null)
                return BadRequest("Failed to deserialize InjestPilotResult = null");

            if (result.InjestRaceId != injestRaceId)
                return BadRequest($"injestRaceId missamch {injestRaceId} != {result.InjestRaceId}");
            if (result.InjestPilotId != injestPilotId)
                return BadRequest($"injestRaceId missamch {injestPilotId} != {result.InjestPilotId}");

            queue.Enqueue(result);
            return Ok();
        }
    }
}
