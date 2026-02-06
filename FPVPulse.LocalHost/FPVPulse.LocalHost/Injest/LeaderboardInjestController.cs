using Microsoft.AspNetCore.Mvc;
using FPVPulse.Ingest;
using FPVPulse.LocalHost.Injest.Db;

namespace FPVPulse.LocalHost.Injest
{
	[Route("api/injest/leaderboard")]
	[ApiController]
	public class LeaderboardInjestController : Controller
	{
		readonly InjestQueue queue;
		readonly InjestData data;

		public LeaderboardInjestController(InjestQueue queue, InjestData data)
		{
			this.queue = queue;
			this.data = data;
		}

		bool TryGetInjestId(out string injestId)
		{
			injestId = Request.Headers["Injest-ID"].FirstOrDefault() ?? "";
			return !string.IsNullOrWhiteSpace(injestId);
		}

		string GetInjestId()
		{
			string? injestId = Request.Headers["Injest-ID"].FirstOrDefault();
			if (injestId == null)
				injestId = "";
			return injestId;
		}

		[HttpPut("{injestEventId}")]
		public async Task<IActionResult> Put(string injestEventId, [FromBody] InjestLeaderboard leaderboard)
		{
			if (string.IsNullOrWhiteSpace(injestEventId))
				return BadRequest("Missing injestRaceId.");

			if (leaderboard == null)
				return BadRequest("Failed to deserialize InjestLeaderboard = null");

			if (leaderboard.InjestEventId != injestEventId)
				return BadRequest($"injestRaceId missamch {injestEventId} != {leaderboard.InjestEventId}");

			queue.Enqueue(GetInjestId(), leaderboard);
			return Ok();
		}
	}
}
