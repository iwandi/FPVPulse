using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest.Db;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FPVPulse.LocalHost.RaceEvent
{
	[Route("api")]
	[ApiController]
	public class LeaderboardController : ControllerBase
	{
		readonly IServiceProvider serviceProvider;

		public LeaderboardController(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		[HttpGet("leaderboard/{leaderboardId}")]
		public ActionResult<Leaderboard> GetEvent(int leaderboardId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var leaderboard = db.Leaderboards.Find(leaderboardId);

			if (leaderboard == null)
				return NotFound();
			return leaderboard;
		}

		[HttpGet("event/{eventId}/leaderabordIndex/")]
		public IEnumerable<IndexEntry> GetEventLeaderboardIndex(int eventId)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			return db.Leaderboards.Where(e => e.EventId == eventId).Select(e => new IndexEntry
			{
				Id = e.LeaderboardId,
				Name = e.RaceType.ToString(),
			}).ToArray();
		}

		[HttpGet("event/{eventId}/leaderabord/{raceType}")]
		public ActionResult<Leaderboard> GetEventLeaderboardByRaceType(int eventId, RaceType raceType)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			var leaderboard = db.Leaderboards.Where(e => e.EventId == eventId && e.RaceType == raceType).FirstOrDefault();

			if (leaderboard == null)
				return NotFound();

			return leaderboard;
		}
	}
}
