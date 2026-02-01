using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FPVPulse.LocalHost.Injest
{
    [Route("api/injest/race")]
    [ApiController]
    public class PilotResultInjestController : Controller
    {
        Dictionary<string, Dictionary<string, string>> echoStorrage = new Dictionary<string, Dictionary<string, string>>();

        [HttpGet("{injestRaceId}/result/{injestPilotId}")]
        public string? Get(string injestRaceId, string injestPilotId)
        {
            if (string.IsNullOrWhiteSpace(injestRaceId) || string.IsNullOrWhiteSpace(injestRaceId))
            {
                throw new ArgumentNullException();
            }

            if (echoStorrage.TryGetValue(injestRaceId, out var raceResults) &&
                raceResults.TryGetValue(injestPilotId, out var value))
            {
                return value;
            }
            return null;
        }

        [HttpPut("{injestRaceId}/result/{injestPilotId}")]
        public void Put(string injestRaceId, string injestPilotId, [FromBody] string value)
        {
            if(string.IsNullOrWhiteSpace(injestRaceId) || string.IsNullOrWhiteSpace(injestRaceId))
            {
                throw new ArgumentNullException();
            }

            if (!echoStorrage.TryGetValue(injestRaceId, out var raceResults))
            {
                raceResults = new Dictionary<string, string>();
                echoStorrage[injestRaceId] = raceResults;
            }
            else
            {
                echoStorrage[injestRaceId][injestPilotId] = value;
            }
        }
    }
}
