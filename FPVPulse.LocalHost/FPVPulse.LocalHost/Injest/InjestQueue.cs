using FPVPulse.Ingest;
using Newtonsoft.Json;

namespace FPVPulse.LocalHost.Injest
{
    public class InjestQueue
    {
        private readonly ILogger<InjestQueue> logger;

        public InjestQueue(ILogger<InjestQueue> logger)
        {
            this.logger = logger;
        }

        public void Enqueue(InjestEvent @event)
        {
            var json = JsonConvert.SerializeObject(@event);
            logger.LogInformation(json);
        }

        public void Enqueue(InjestRace race)
        {
            var json = JsonConvert.SerializeObject(race);
            logger.LogInformation(json);
        }

        public void Enqueue(InjestPilotResult pilotResult)
        {
            var json = JsonConvert.SerializeObject(pilotResult);
            logger.LogInformation(json);
        }
    }
}
