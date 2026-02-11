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
		public LeaderboardDataTransfromer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{
		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnInjestLeaderboardChanged += OnChanged;
		}

		protected override async Task CheckExisting(EventDbContext db, InjestDbContext injestDb)
		{
			foreach (var leaderboard in injestDb.Leaderboard)
			{
				//InjestData.FillResult(injestDb, leaderboard);

				while(!await Process(db, leaderboard, leaderboard.LeaderboardId, leaderboard.EventId.Value))
				{

				}
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

			// TODO : fill result 

			if(leaderboardHasChange)
				await changeSignaler.SignalChangeAsync(ChangeGroup.Leaderboard, existingLeaderboard.LeaderboardId, 0, existingLeaderboard);

			return true;
		}

		void WriteData(Leaderboard leaderboard, DbInjestLeaderboard injestLeaderboard)
		{
			leaderboard.RaceType = injestLeaderboard.RaceType ?? RaceType.Unknown;
		}
	}
}
