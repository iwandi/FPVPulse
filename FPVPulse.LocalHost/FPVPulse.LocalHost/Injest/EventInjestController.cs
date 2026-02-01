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
        private readonly InjestQueue queue;
        private readonly InjestData data;

        public EventInjestController(InjestQueue queue, InjestData data)
        {
            this.queue = queue;
            this.data = data;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return data.GetEventIds();
        }

        [HttpGet("{injestEventId}")]
        public ActionResult<InjestEvent> Get(string injestEventId)
        {
            if (string.IsNullOrWhiteSpace(injestEventId))
                return BadRequest("Missing injestEventId.");

            var @event = data.GetEvent(injestEventId);
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

            queue.Enqueue(@event);
            return Ok();
        }
    }
}
