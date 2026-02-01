using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FPVPulse.LocalHost.Injest
{
    [Route("api/injest/race")]
    [ApiController]
    public class RaceInjestController : ControllerBase
    {
        Dictionary<string, string> echoStorrage = new Dictionary<string, string>();

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return echoStorrage.Keys;
        }

        [HttpGet("{injestRaceId}")]
        public string Get(string injestRaceId)
        {
            if (string.IsNullOrWhiteSpace(injestRaceId))
            {
                throw new ArgumentNullException(nameof(injestRaceId));
            }

            if (echoStorrage.TryGetValue(injestRaceId, out var value))
            {
                return value;
            }
            return null;
        }

        // TODO : how to we structure the id mapping from the Injest ids to the acctual ids ?
        [HttpPut("{injestRaceId}")]
        public void Put(string injestRaceId, [FromBody] string value)
        {
            echoStorrage[injestRaceId] = value;
        }
    }
}
