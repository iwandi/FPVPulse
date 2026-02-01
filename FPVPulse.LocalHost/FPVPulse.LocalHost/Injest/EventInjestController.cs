using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FPVPulse.LocalHost.Injest
{
    [Route("api/injest/event")]
    [ApiController]
    public class EventInjestController : Controller
    {
        Dictionary<string, string> echoStorrage = new Dictionary<string, string>();

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return echoStorrage.Keys;
        }

        [HttpGet("{injestEventId}")]
        public string Get(string injestEventId)
        {
            if (string.IsNullOrWhiteSpace(injestEventId))
            {
                throw new ArgumentNullException(nameof(injestEventId));
            }

            if (echoStorrage.TryGetValue(injestEventId, out var value))
            {
                return value;
            }
            return null;
        }

        [HttpPut("{injestEventId}")]
        public void Put(string injestEventId, [FromBody] string value)
        {
            if (string.IsNullOrWhiteSpace(injestEventId))
            {
                throw new ArgumentNullException(nameof(injestEventId));
            }

            echoStorrage[injestEventId] = value;
        }
    }
}
