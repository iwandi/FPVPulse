using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
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
				else if(queue.TryDequeueLeaderabord(out injestId, out var leaderboard))
					await ProcessLeaderabord(db, injestId, leaderboard);

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
            await signaler.SignalChangeAsync(ChangeGroup.InjestEvent, existing.EventId, -1, existing);
        }

        async Task ProcessRace(InjestDbContext? db, string injestId, InjestRace race)
        {
            bool hasChange = false;
            var existing = await db.Races.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                e.InjestEventId == race.InjestEventId &&
                e.InjestRaceId == race.InjestRaceId);

            var @event = await db.Events.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                e.InjestEventId == race.InjestEventId);

            if (existing == null)
            {
                existing = new DbInjestRace(injestId, race, @event);
                db.Races.Add(existing);
                hasChange = true;
            }
            else
                hasChange |= existing.Merge(race, @event);

            if(hasChange)
            {
                var json = JsonConvert.SerializeObject(existing);
                logger.LogInformation(json);

                await db.SaveChangesAsync();
            }

            List<DbInjestRacePilot> existingPilots = new List<DbInjestRacePilot>();
			List<DbInjestRacePilot> changedPilots = new List<DbInjestRacePilot>();
			if (race.Pilots != null)
            {
                foreach (var pilot in race.Pilots)
                {
                    bool pilotHasChanges = false;
                    var existingPilot = await db.RacePilots.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                        e.InjestPilotEntryId == pilot.InjestPilotEntryId &&
                        e.InjestRaceId == existing.InjestRaceId);

                    if (existingPilot == null)
                    {
                        existingPilot = new DbInjestRacePilot(injestId, pilot, existing);
                        db.RacePilots.Add(existingPilot);
                        pilotHasChanges |= true;
                    }
                    else
                        pilotHasChanges |= existingPilot.Merge(pilot);

                    if(pilotHasChanges)
                    {
                        var pilotJson = JsonConvert.SerializeObject(existingPilot);
                        logger.LogInformation(pilotJson);

                        await db.SaveChangesAsync();
                        changedPilots.Add(existingPilot);
					}
					existingPilots.Add(existingPilot);

					hasChange |= pilotHasChanges;
                }
            }

            if (hasChange)
            {
                existing.Pilots = existingPilots.ToArray();

				var eventId = @event != null ? @event.EventId : -1;
                await signaler.SignalChangeAsync(ChangeGroup.InjestRace, existing.RaceId, eventId, existing);

				foreach (var pilot in changedPilots)
					await signaler.SignalChangeAsync(ChangeGroup.InjestRacePilot, pilot.RacePilotId, existing.RaceId, pilot);
			}
        }

        async Task ProcessPilotResult(InjestDbContext? db, string injestId, InjestPilotResult pilotResult)
        {
            bool hasPilotId = pilotResult.InjestPilotId != null && !string.IsNullOrWhiteSpace(pilotResult.InjestPilotId);
			bool hasPilotEntryId = pilotResult.InjestPilotEntryId != null && !string.IsNullOrWhiteSpace(pilotResult.InjestPilotEntryId);

            DbInjestPilotResult? existing = null;
            if (hasPilotId)
            {
                existing = await db.PilotResults.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                    e.InjestRaceId == pilotResult.InjestRaceId &&
                    e.InjestPilotId == pilotResult.InjestPilotId);
            }
			else if(hasPilotEntryId)
			{
				existing = await db.PilotResults.FirstOrDefaultAsync(e => e.InjestId == injestId &&
					e.InjestRaceId == pilotResult.InjestRaceId &&
					e.InjestPilotEntryId == pilotResult.InjestPilotEntryId);
			}
            else
                throw new Exception("PilotResult must have either InjestPilotId or InjestPilotEntryId");

			var race = await db.Races.FirstOrDefaultAsync(e => e.InjestId == injestId &&
                        e.InjestRaceId == pilotResult.InjestRaceId);

            if (existing == null)
            {
                existing = new DbInjestPilotResult(injestId, pilotResult, race);
                db.PilotResults.Add(existing);
            }
            else
            {
                if (!existing.Merge(pilotResult, race))
                    return;
            }

            var json = JsonConvert.SerializeObject(existing);
            logger.LogInformation(json);

            await db.SaveChangesAsync();

            var raceId = race != null ? race.RaceId : -1;
            await signaler.SignalChangeAsync(ChangeGroup.InjestPilotResult, existing.PilotResultId, raceId, existing);
        }

        async Task ProcessLeaderabord(InjestDbContext? db, string injestId, InjestLeaderboard leaderboard)
        {
			bool hasChange = false;
            bool hasLeaderboardId = leaderboard.InjestLeaderboardId != null && !string.IsNullOrWhiteSpace(leaderboard.InjestLeaderboardId);
            DbInjestLeaderboard? existing = null;
			if (hasLeaderboardId)
            {
				existing = await db.Leaderboard.FirstOrDefaultAsync(e => e.InjestId == injestId &&
					e.InjestEventId == leaderboard.InjestEventId &&
					e.InjestLeaderboardId == leaderboard.InjestLeaderboardId);
			}
            else
            {
				existing = await db.Leaderboard.FirstOrDefaultAsync(e => e.InjestId == injestId &&
					e.InjestEventId == leaderboard.InjestEventId &&
					e.RaceType == leaderboard.RaceType);
			}

			var @event = await db.Events.FirstOrDefaultAsync(e => e.InjestId == injestId &&
				e.InjestEventId == leaderboard.InjestEventId);

			if (existing == null)
			{
				existing = new DbInjestLeaderboard(injestId, leaderboard, @event);
				db.Leaderboard.Add(existing);
				hasChange = true;
			}
			else
				hasChange |= existing.Merge(leaderboard, @event);

			if (hasChange)
			{
				var json = JsonConvert.SerializeObject(existing);
				logger.LogInformation(json);

				await db.SaveChangesAsync();
			}

			List<DbInjestLeaderboardPilot> existingPilots = new List<DbInjestLeaderboardPilot>();
			List<DbInjestLeaderboardPilot> changedPilots = new List<DbInjestLeaderboardPilot>();
			if (leaderboard.Results != null)
			{
				foreach (var pilot in leaderboard.Results)
				{
					bool pilotHasChanges = false;

					bool hasPilotId = pilot.InjestPilotId != null && !string.IsNullOrWhiteSpace(pilot.InjestPilotId);
					bool hasPilotEntryId = pilot.InjestPilotEntryId != null && !string.IsNullOrWhiteSpace(pilot.InjestPilotEntryId);

					DbInjestLeaderboardPilot? existingPilot = null;
					if (hasPilotId)
					{
						existingPilot = await db.LeaderboardPilots.FirstOrDefaultAsync(e => e.InjestId == injestId &&
							e.LeaderboardId == existing.LeaderboardId &&
							e.InjestPilotId == pilot.InjestPilotId);
					}
					else if (hasPilotEntryId)
					{
						existingPilot = await db.LeaderboardPilots.FirstOrDefaultAsync(e => e.InjestId == injestId &&
							e.LeaderboardId == existing.LeaderboardId &&
							e.InjestPilotEntryId == pilot.InjestPilotEntryId);
					}
					else
						throw new Exception("PilotResult must have either InjestPilotId or InjestPilotEntryId");

					if (existingPilot == null)
					{
						existingPilot = new DbInjestLeaderboardPilot(injestId, pilot, existing);
						db.LeaderboardPilots.Add(existingPilot);
						pilotHasChanges |= true;
					}
					else
						pilotHasChanges |= existingPilot.Merge(pilot);

					if (pilotHasChanges)
					{
						var pilotJson = JsonConvert.SerializeObject(existingPilot);
						logger.LogInformation(pilotJson);

						await db.SaveChangesAsync();
						changedPilots.Add(existingPilot);
					}
					existingPilots.Add(existingPilot);

					hasChange |= pilotHasChanges;
				}
			}

			if (hasChange)
			{
				existing.Results = existingPilots.ToArray();

				var eventId = @event != null ? @event.EventId : -1;
				await signaler.SignalChangeAsync(ChangeGroup.InjestLeaderboard, existing.LeaderboardId, eventId, existing);

				foreach (var pilot in changedPilots)
					await signaler.SignalChangeAsync(ChangeGroup.InjestLeaderboardPilot, pilot.LeaderboardId, existing.LeaderboardId, pilot);
			}
		}
    }
}
