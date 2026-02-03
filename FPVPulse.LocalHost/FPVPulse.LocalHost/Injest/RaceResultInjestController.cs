using FPVPulse.Ingest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FPVPulse.LocalHost.Injest
{
    [Route("api/injest/race")]
    [ApiController]
    public class PilotResultInjestController : Controller
    {
        readonly InjestQueue queue;
        readonly InjestData data;

        public PilotResultInjestController(InjestQueue queue, InjestData data)
        {
            this.queue = queue;
            this.data = data;
        }

        string GetInjestId()
        {
            string? injestId = Request.Headers["Injest-ID"].FirstOrDefault();
            if (injestId == null)
                injestId = "";
            return injestId;
        }

        [HttpGet("{injestRaceId}/result/{injestPilotId}")]
        public ActionResult<InjestPilotResult> Get(string injestRaceId, string injestPilotId)
        {
            if (string.IsNullOrWhiteSpace(injestRaceId))
                return BadRequest("Missing injestRaceId.");

            if (string.IsNullOrWhiteSpace(injestPilotId))
                return BadRequest("Missing injestPilotId.");

            var result = data.GetPilotResult(GetInjestId(), injestRaceId, injestPilotId);
            if (result == null)
                return NotFound();

            return result;
        }

        [HttpPut("{injestRaceId}/result")]
        public async Task<IActionResult> Put(string injestRaceId, [FromBody] InjestPilotResult result)
        {
            if (string.IsNullOrWhiteSpace(injestRaceId))
                return BadRequest("Missing injestRaceId.");

            if (result == null)
                return BadRequest("Failed to deserialize InjestPilotResult = null");

            if (result.InjestRaceId != injestRaceId)
                return BadRequest($"injestRaceId missamch {injestRaceId} != {result.InjestRaceId}");

			bool hasPilotId = result.InjestPilotId != null && !string.IsNullOrWhiteSpace(result.InjestPilotId);
			bool hasPilotEntryId = result.InjestPilotEntryId != null && !string.IsNullOrWhiteSpace(result.InjestPilotEntryId);
			if (!hasPilotId && !hasPilotEntryId)
				return BadRequest("Missing injestPilotId and injestPilotEntryId.");

			queue.Enqueue(GetInjestId(), result);
            return Ok();
        }
    }
}
