using FPVPulse.Ingest;
using FPVPulse.LocalHost.Injest.Db;
using Microsoft.EntityFrameworkCore;

namespace FPVPulse.LocalHost.Injest
{
    public class InjestProcessor : BackgroundService
    {
        readonly InjestQueue queue;
        readonly InjestDbContext db;

        public InjestProcessor(InjestQueue queue, InjestDbContext db)
        {
            this.queue = queue;
            this.db = db;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                string injestId;
                if (queue.TryDequeueEvent(out injestId, out var @event))
                    await ProcessEvent(injestId, @event);
                else if (queue.TryDequeueRace(out injestId, out var race))
                    await ProcessRace(injestId, race);
                else if (queue.TryDequeuePilotResult(out injestId, out var pilotResult))
                    await ProcessPilotResult(injestId, pilotResult);
                else
                    await Task.Delay(100);
            }
        }

        async Task ProcessEvent(string injestId, InjestEvent @event)
        {
            var existing = await db.Events.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                e.InjestEventId == @event.InjestEventId);
            if (existing == null)
            {
                existing = new DbInjestEvent(injestId, @event);
                db.Events.Add(existing);
            }
            else
            {
                if (!existing.Merge(@event))
                    return;
            }

            await db.SaveChangesAsync();
        }

        async Task ProcessRace(string injestId, InjestRace race)
        {
            var existing = await db.Races.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                e.InjestEventId == race.InjestEventId &&
                e.InjestRaceId == race.InjestRaceId);
            if (existing == null)
            {
                existing = new DbInjestRace(injestId, race);
                db.Races.Add(existing);
            }
            else
            {
                if(!existing.Merge(race))
                    return;
            }

            await db.SaveChangesAsync();
        }

        async Task ProcessPilotResult(string injestId, InjestPilotResult pilotResult)
        {
            var existing = await db.PilotResults.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                e.InjestRaceId == pilotResult.InjestRaceId &&
                e.InjestPilotId == pilotResult.InjestPilotId);
            if (existing == null)
            {
                existing = new DbInjestPilotResult(injestId, pilotResult);
                db.PilotResults.Add(existing);
            }
            else
            {
                if (!existing.Merge(pilotResult))
                    return;
            }

            await db.SaveChangesAsync();
        }
    }
}
