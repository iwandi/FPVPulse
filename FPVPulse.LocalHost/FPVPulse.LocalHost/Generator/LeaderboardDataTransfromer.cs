using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.EntityFrameworkCore;

namespace FPVPulse.LocalHost.Generator
{
	public class LeaderboardDataTransfromer : BaseTransformer<DbInjestLeaderboard>
	{
		LeaderboardPilotDataTransformer leaderboardPilotDataTransformer;

		public LeaderboardDataTransfromer(LeaderboardPilotDataTransformer lt, ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{
			leaderboardPilotDataTransformer = lt;
		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestLeaderboardChanged += OnChanged;
		}

		protected override async Task CheckExisting(EventDbContext db, InjestDbContext injestDb)
		{
			foreach (var leaderboard in injestDb.Leaderboard)
			{
				await ProcessUntilDone(db, leaderboard, leaderboard.LeaderboardId, leaderboard.EventId.Value);
			}
		}

		protected override async Task<bool> Process(EventDbContext db, DbInjestLeaderboard data, int id, int parentId)
		{
			var @event = await db.Events.Where(e => e.InjestEventId == parentId).FirstOrDefaultAsync();

			if(@event == null)
				return false;

			var existingLeaderboard = await db.Leaderboards.Where(l => l.InjestLeaderboardId == id).FirstOrDefaultAsync();

			if (existingLeaderboard == null)
			{
				existingLeaderboard = new Leaderboard { InjestLeaderboardId = id, EventId = @event.EventId };
				WriteData(existingLeaderboard, data);
				db.Leaderboards.Add(existingLeaderboard);
			}
			else
				WriteData(existingLeaderboard, data);

			var leaderboardHasChange = await db.SaveChangesAsync() > 0;

			await leaderboardPilotDataTransformer.WaitForAllParentsDone(id);

			await EventDbContext.FillLeaderboard(db, existingLeaderboard);

			if (leaderboardHasChange)
				await changeSignaler.SignalChangeAsync(ChangeGroup.Leaderboard, existingLeaderboard.LeaderboardId, 0, existingLeaderboard);

			return true;
		}

		void WriteData(Leaderboard leaderboard, DbInjestLeaderboard injestLeaderboard)
		{
			leaderboard.RaceType = injestLeaderboard.RaceType ?? RaceType.Unknown;
		}
	}
}
