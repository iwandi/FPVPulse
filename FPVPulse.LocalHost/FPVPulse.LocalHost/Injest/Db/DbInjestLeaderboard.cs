using FPVPulse.Ingest;
using Microsoft.EntityFrameworkCore.Query;
using System.ComponentModel.DataAnnotations;

namespace FPVPulse.LocalHost.Injest.Db
{
	public class DbInjestLeaderboard : InjestLeaderboard
	{
		[Key]
		public int LeaderboardId { get; set; }

		public int? EventId { get; set; }

		[Required]
		public string InjestId { get; set; } = string.Empty;

		public DbInjestLeaderboard()
		{

		}

		public DbInjestLeaderboard(string injestId, InjestLeaderboard leaderboard, DbInjestEvent? @event)
		{
			InjestId = injestId;

			if (@event != null)
				EventId = @event.EventId;

			InjestEventId = leaderboard.InjestEventId;
			RaceType = leaderboard.RaceType;

			Results = leaderboard.Results;
		}

		public bool Merge(InjestLeaderboard leaderboard, DbInjestEvent? @event)
		{
			bool changed = false;
			if(leaderboard.RaceType != null && RaceType != leaderboard.RaceType)
			{
				RaceType = leaderboard.RaceType;
				changed = true;
			}

			if (@event != null && EventId != @event.EventId)
			{
				EventId = @event.EventId;
				changed = true;
			}
			return changed;
		}
	}
}
