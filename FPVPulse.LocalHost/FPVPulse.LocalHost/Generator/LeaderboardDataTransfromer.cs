using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Client.Components.Data;
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
			changeSignaler.OnInjestLeaderabordChanged += OnChanged;
		}

		protected override async Task Process(EventDbContext db, DbInjestLeaderboard data, int id, int parentId)
		{
			var existingLeaderboard = await db.Leaderboards.Where(l => l.InjestLeaderboardId == id).FirstOrDefaultAsync();

			if (existingLeaderboard == null)
			{
				existingLeaderboard = new Leaderboard { InjestLeaderboardId = id };
				WriteData(existingLeaderboard, data);
				db.Leaderboards.Add(existingLeaderboard);
			}
			else
				WriteData(existingLeaderboard, data);

			var leaderboardHasChange = await db.SaveChangesAsync() > 0;

			List<Pilot> changedPilot = new List<Pilot>();
			List<LeaderboardPilot> pilots = new List<LeaderboardPilot>();
			List<LeaderboardPilot> changedLeaderboardPilot = new List<LeaderboardPilot>();
			if (data.Results != null && data.Results.Length > 0)
			{
				foreach (var injestResult in data.Results)
				{
					var dataPilot = injestResult as DbInjestLeaderboardPilot;
					if (dataPilot == null)
						throw new Exception("Unexpected type DbInjestLeaderboard contains non DbInjestLeaderboardPilot in Results");

					var injestPilotId = dataPilot.InjestPilotId;
					var (existingPilot, pilotHasChanged) = await db.MatchPilot(injestPilotId, dataPilot.InjestName);

					var leaderboardPilotInjestId = dataPilot.LeaderboardPilotId;
					var existingLeaderboardPilot = await db.LeaderboardPilots.Where(lp => lp.InjestLeaderboardPilotId == leaderboardPilotInjestId).FirstOrDefaultAsync();
					if (existingLeaderboardPilot == null)
					{
						existingLeaderboardPilot = new LeaderboardPilot { LeaderboardId = existingLeaderboard.LeaderboardId, InjestLeaderboardPilotId = leaderboardPilotInjestId, PilotId = existingPilot.PilotId };
						WriteData(existingLeaderboardPilot, dataPilot, existingPilot.PilotId, parentId);
						db.LeaderboardPilots.Add(existingLeaderboardPilot);
					}
					else
						WriteData(existingLeaderboardPilot, dataPilot, existingPilot.PilotId, parentId);

					var leaderboardPilotHasChanges = await db.SaveChangesAsync() > 0;

					existingLeaderboardPilot.Pilot = existingPilot;
					pilots.Add(existingLeaderboardPilot);

					if (pilotHasChanged)
						changedPilot.Add(existingPilot);
					if (leaderboardPilotHasChanges)
						changedLeaderboardPilot.Add(existingLeaderboardPilot);
				}
			}

			existingLeaderboard.Pilots = pilots.ToArray();

			if(leaderboardHasChange)
				await changeSignaler.SignalChangeAsync(ChangeGroup.Leaderboard, existingLeaderboard.LeaderboardId, 0, existingLeaderboard);
			foreach(var pilot in changedPilot)
				await changeSignaler.SignalChangeAsync(ChangeGroup.Pilot, pilot.PilotId, 0, pilot);
			foreach (var leaderboardPilot in changedLeaderboardPilot)
				await changeSignaler.SignalChangeAsync(ChangeGroup.LeaderboardPilot, leaderboardPilot.LeaderboardPilotId, leaderboardPilot.LeaderboardId, leaderboardPilot);
		}

		void WriteData(Leaderboard leaderboard, DbInjestLeaderboard injestLeaderboard)
		{
			leaderboard.RaceType = injestLeaderboard.RaceType ?? RaceType.Unknown;
		}

		void WriteData(LeaderboardPilot leaderboardPilot, DbInjestLeaderboardPilot injestLeaderboardPilot, int pilotId, int positionReasonRaceId)
		{
			leaderboardPilot.PilotId = pilotId;
			leaderboardPilot.Position = injestLeaderboardPilot.Position;
			leaderboardPilot.PositionDelta = injestLeaderboardPilot.PositionDelta;
			leaderboardPilot.PositionReason = injestLeaderboardPilot.PositionReason;
			leaderboardPilot.PositionReasonRaceId = positionReasonRaceId;
			leaderboardPilot.Flags = injestLeaderboardPilot.Flags;
		}
	}
}
