using FPVPulse.Ingest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace FPVPulse.LocalHost.Injest
{
    [Route("api/injest/event")]
    [ApiController]
    public class EventInjestController : Controller
    {
        readonly InjestQueue queue;
        readonly InjestData data;

        public EventInjestController(InjestQueue queue, InjestData data)
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

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return data.GetEventIds(GetInjestId());
        }

        [HttpGet("{injestEventId}")]
        public ActionResult<InjestEvent> Get(string injestEventId)
        {
            if (string.IsNullOrWhiteSpace(injestEventId))
                return BadRequest("Missing injestEventId.");

            var @event = data.GetEvent(GetInjestId(), injestEventId);
            if (@event == null)
                return NotFound();

            return @event;
        }

        [HttpPut("{injestEventId}")]
        public async Task<IActionResult> Put(string injestEventId, [FromBody] InjestEvent @event)
        {
            if (string.IsNullOrWhiteSpace(injestEventId))
                return BadRequest("Invalid injestEventId");

            if (@event == null)
                return BadRequest("Invalid event data.");

            if (@event.InjestEventId != injestEventId)
                return BadRequest("Event ID in URL and body do not match.");

            queue.Enqueue(GetInjestId(), @event);
            return Ok();
        }
    }
}
