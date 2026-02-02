using FPVPulse.Ingest;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FPVPulse.LocalHost.Injest
{
    public class InjestProcessor : BackgroundService
    {
        readonly IServiceProvider serviceProvider;
        readonly InjestQueue queue;
        readonly ChangeSignaler signaler;

        readonly ILogger<InjestProcessor> logger;

        public InjestProcessor(InjestQueue queue, ChangeSignaler signaler, ILogger<InjestProcessor> logger, IServiceProvider serviceProvider)
        {
            this.queue = queue;
            this.signaler = signaler;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

            while (!stoppingToken.IsCancellationRequested)
            {
                string injestId;
                if (queue.TryDequeueEvent(out injestId, out var @event))
                    await ProcessEvent(db, injestId, @event);
                else if (queue.TryDequeueRace(out injestId, out var race))
                    await ProcessRace(db, injestId, race);
                else if (queue.TryDequeuePilotResult(out injestId, out var pilotResult))
                    await ProcessPilotResult(db, injestId, pilotResult);

                await queue.WaitForAnyAsync(stoppingToken);
            }
        }

        async Task ProcessEvent(InjestDbContext? db, string injestId, InjestEvent @event)
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

            var json = JsonConvert.SerializeObject(existing);
            logger.LogInformation(json);

            await db.SaveChangesAsync();
            await signaler.SignalChangeAsync(ChangeGroup.InjestEvent, existing.EventId);
        }

        async Task ProcessRace(InjestDbContext? db, string injestId, InjestRace race)
        {
            bool hasChange = false;
            var existing = await db.Races.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                e.InjestEventId == race.InjestEventId &&
                e.InjestRaceId == race.InjestRaceId);
            if (existing == null)
            {
                existing = new DbInjestRace(injestId, race);
                db.Races.Add(existing);
                hasChange = true;
            }
            else
                hasChange &= existing.Merge(race);

            if(hasChange)
            {
                var json = JsonConvert.SerializeObject(existing);
                logger.LogInformation(json);

                await db.SaveChangesAsync();
            }

            if (race.Pilots != null)
            {
                foreach (var pilot in race.Pilots)
                {
                    bool pilotHasChanges = false;
                    var existingPilot = await db.RacePilots.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                        e.InjestPilotId == pilot.InjestPilotId &&
                        e.InjestRaceId == existing.InjestRaceId);

                    if (existingPilot == null)
                    {
                        existingPilot = new DbInjestRacePilot(injestId, existing, pilot);
                        db.RacePilots.Add(existingPilot);
                        pilotHasChanges = true;
                    }
                    else
                        pilotHasChanges &= existingPilot.Merge(pilot);

                    if(pilotHasChanges)
                    {
                        var pilotJson = JsonConvert.SerializeObject(existingPilot);
                        logger.LogInformation(pilotJson);

                        await db.SaveChangesAsync();
                    }

                    hasChange &= pilotHasChanges;
                }
            }

            if (hasChange)
            {
                await signaler.SignalChangeAsync(ChangeGroup.InjestRace, existing.RaceId);

                // We need to wait to make shure all writes to all pilots are done. 
                /*if (race.Pilots != null)
                {
                    foreach (var pilot in race.Pilots)
                    {
                        bool writeDone = false;
                        while (!writeDone)
                        {
                            var existingPilot = await db.RacePilots.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                                e.InjestPilotId == pilot.InjestPilotId &&
                                e.InjestRaceId == existing.InjestRaceId);

                            writeDone = existingPilot != null;
                        }
                    }
                }*/
            }
        }

        async Task ProcessPilotResult(InjestDbContext? db, string injestId, InjestPilotResult pilotResult)
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

            var json = JsonConvert.SerializeObject(existing);
            logger.LogInformation(json);

            await db.SaveChangesAsync();
            await signaler.SignalChangeAsync(ChangeGroup.InjestPilotResult, existing.PilotResultId);
        }
    }
}
