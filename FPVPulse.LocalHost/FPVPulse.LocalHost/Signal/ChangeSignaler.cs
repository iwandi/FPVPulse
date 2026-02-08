using Microsoft.AspNetCore.SignalR;
using FPVPulse.LocalHost.Client;
using Microsoft.AspNetCore.Components;
using FPVPulse.Ingest;
using FPVPulse.LocalHost.Injest.Db;

namespace FPVPulse.LocalHost.Signal
{
    public class ChangeSignaler
    {
        public event EventHandler<ChangeEventArgs<object>>? OnChange;
        public event EventHandler<ChangeEventArgs<DbInjestEvent>>? OnInjestEventChanged;
        public event EventHandler<ChangeEventArgs<DbInjestRace>>? OnInjestRaceChanged;
		public event EventHandler<ChangeEventArgs<DbInjestRacePilot>>? OnInjestRaceDataChanged;
		public event EventHandler<ChangeEventArgs<DbInjestPilotResult>>? OnInjestPilotResultChanged;
		public event EventHandler<ChangeEventArgs<DbInjestLeaderboard>>? OnInjestLeaderabordChanged;
		public event EventHandler<ChangeEventArgs<DbInjestLeaderboardPilot>>? OnInjestLeaderabordPilotChanged;

		readonly IHubContext<ChangeHub> changeHub;

        public ChangeSignaler(IHubContext<ChangeHub> changeHub)
        {
            this.changeHub = changeHub;
        }

        public async Task SignalChangeAsync<T>(ChangeGroup group, int id, int parentId, T data)
        {
            var change = new ChangeEventArgs<T>(group, id, parentId, data);
            await SignalChangeAsync(change);
        }

		public async Task SignalChangeAsync<T>(ChangeEventArgs<T> change)
        {
			OnChange?.Invoke(this, new ChangeEventArgs<object>(
	            change.Group,
	            change.Id,
	            change.ParentId,
	            change.Data
            ));

			var group = string.Empty;
            var groupData = string.Empty;
			var dataMethod = string.Empty;
			switch (change.Group)
            {
                case ChangeGroup.InjestEvent:
				case ChangeGroup.InjestEventData:
					if (change is ChangeEventArgs<DbInjestEvent> eventArgs)
						OnInjestEventChanged?.Invoke(this, eventArgs);
					group = ChangeGroup.InjestEvent.ToString();
                    groupData = ChangeGroup.InjestEventData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestEventData;
					break;
                case ChangeGroup.InjestRace:
				case ChangeGroup.InjestRaceData:
					if (change is ChangeEventArgs<DbInjestRace> raceArgs)
						OnInjestRaceChanged?.Invoke(this, raceArgs);
					group = ChangeGroup.InjestRace.ToString();
					groupData = ChangeGroup.InjestRaceData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestRaceData;
					break;
				case ChangeGroup.InjestRacePilot:
				case ChangeGroup.InjestRacePilotData:
					if (change is ChangeEventArgs<DbInjestRacePilot> racePilotArgs)
						OnInjestRaceDataChanged?.Invoke(this, racePilotArgs);
					group = ChangeGroup.InjestRacePilot.ToString();
					groupData = ChangeGroup.InjestRacePilotData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestRacePilotData;
					break;
				case ChangeGroup.InjestPilotResult:
				case ChangeGroup.InjestPilotResultData:
					if (change is ChangeEventArgs<DbInjestPilotResult> pilotResultArgs)
						OnInjestPilotResultChanged?.Invoke(this, pilotResultArgs);
					group = ChangeGroup.InjestPilotResult.ToString();
					groupData = ChangeGroup.InjestPilotResultData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestPoilotResultData;
					break;
				case ChangeGroup.InjestLeaderboard:
				case ChangeGroup.InjestLeaderboardData:
					if (change is ChangeEventArgs<DbInjestLeaderboard> leaderabordArgs)
						OnInjestLeaderabordChanged?.Invoke(this, leaderabordArgs);
					group = ChangeGroup.InjestLeaderboard.ToString();
					groupData = ChangeGroup.InjestLeaderboardData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestLeaderboardData;
					break;
				case ChangeGroup.InjestLeaderboardPilot:
				case ChangeGroup.InjestLeaderboardPilotData:
					if (change is ChangeEventArgs<DbInjestLeaderboardPilot> leaderabordPilotArgs)
						OnInjestLeaderabordPilotChanged?.Invoke(this, leaderabordPilotArgs);
					group = ChangeGroup.InjestLeaderboardPilot.ToString();
					groupData = ChangeGroup.InjestLeaderboardPilotData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestLeaderboardPilotData;
					break;
			}

			await changeHub.Clients.Group(group)
                .SendAsync(ChangeSignalMessages.Change, group, change.Id, change.ParentId);
			await changeHub.Clients.Group(groupData)
				.SendAsync(dataMethod, change.Id, change.ParentId, change.Data);
		}
    }
}
