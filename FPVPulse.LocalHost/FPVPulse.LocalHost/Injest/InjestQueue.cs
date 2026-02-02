using FPVPulse.Ingest;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace FPVPulse.LocalHost.Injest
{
    public class InjestQueue
    {
        class QueueItem<T>
        {
            public string InjestId { get; set; } = string.Empty;
            public T Item { get; set; }
        }

        readonly ConcurrentQueue<QueueItem<InjestEvent>> eventQueue = new();
        readonly ConcurrentQueue<QueueItem<InjestRace>> raceQueue = new();
        readonly ConcurrentQueue<QueueItem<InjestPilotResult>> pilotResultQueue = new();
        readonly SemaphoreSlim signal = new(0);

        public bool HasAnyItem => !eventQueue.IsEmpty ||
            !raceQueue.IsEmpty ||
            !pilotResultQueue.IsEmpty;

        //readonly ILogger<InjestQueue> logger;

        public InjestQueue(ILogger<InjestQueue> logger)
        {
            //this.logger = logger;
        }

        public void Enqueue(string injestId, InjestEvent @event)
        {
            /*var json = JsonConvert.SerializeObject(@event);
            logger.LogInformation(injestId);
            logger.LogInformation(json);*/

            eventQueue.Enqueue(new QueueItem<InjestEvent>
            {
                InjestId = injestId,
                Item = @event
            });
            signal.Release();
        }

        public bool TryDequeueEvent(out string injestId, out InjestEvent @event)
        {
            if (eventQueue.TryDequeue(out var item))
            {
                injestId = item.InjestId;
                @event = item.Item;
                return true;
            }
            injestId = string.Empty;
            @event = null!;
            return false;
        }

        public void Enqueue(string injestId, InjestRace race)
        {
            /*var json = JsonConvert.SerializeObject(race);
            logger.LogInformation(injestId);
            logger.LogInformation(json);*/

            raceQueue.Enqueue(new QueueItem<InjestRace> { 
                InjestId = injestId,  
                Item = race
            });
            signal.Release();
        }

        public bool TryDequeueRace(out string injestId, out InjestRace race)
        {
            if (raceQueue.TryDequeue(out var item))
            {
                injestId = item.InjestId;
                race = item.Item;
                return true;
            }
            injestId = string.Empty;
            race = null!;
            return false;
        }

        public void Enqueue(string injestId,  InjestPilotResult pilotResult)
        {
            /*var json = JsonConvert.SerializeObject(pilotResult);
            logger.LogInformation(injestId);
            logger.LogInformation(json);*/

            pilotResultQueue.Enqueue(new QueueItem<InjestPilotResult>
            {
                InjestId = injestId,
                Item = pilotResult
            });
            signal.Release();
        }

        public bool TryDequeuePilotResult(out string injestId, out InjestPilotResult pilotResult)
        {
            if (pilotResultQueue.TryDequeue(out var item))
            {
                injestId = item.InjestId;
                pilotResult = item.Item;
                return true;
            }
            injestId = string.Empty;
            pilotResult = null!;
            return false;
        }

        public async Task WaitForAnyAsync(CancellationToken token)
        {
            while (!HasAnyItem)
            {
                await signal.WaitAsync(token);
            }
        }
    }
}
