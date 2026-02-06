using Microsoft.AspNetCore.SignalR;
using FPVPulse.LocalHost.Client;
using Microsoft.AspNetCore.Components;
using FPVPulse.Ingest;

namespace FPVPulse.LocalHost.Signal
{
    public class ChangeSignaler
    {
        public event EventHandler<ChangeEventArgs<object>>? OnChange;
        public event EventHandler<ChangeEventArgs<InjestEvent>>? OnInjestEventChanged;
        public event EventHandler<ChangeEventArgs<InjestRace>>? OnInjestRaceChanged;
		public event EventHandler<ChangeEventArgs<InjestRacePilot>>? OnInjestRaceDataChanged;
		public event EventHandler<ChangeEventArgs<InjestPilotResult>>? OnInjestPilotResultChanged;

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
					if (change is ChangeEventArgs<InjestEvent> eventArgs)
						OnInjestEventChanged?.Invoke(this, eventArgs);
					group = ChangeGroup.InjestEvent.ToString();
                    groupData = ChangeGroup.InjestEventData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestEventData;
					break;
                case ChangeGroup.InjestRace:
				case ChangeGroup.InjestRaceData:
					if (change is ChangeEventArgs<InjestRace> raceArgs)
						OnInjestRaceChanged?.Invoke(this, raceArgs);
					group = ChangeGroup.InjestRace.ToString();
					groupData = ChangeGroup.InjestRaceData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestRaceData;
					break;
				case ChangeGroup.InjestRacePilot:
				case ChangeGroup.InjestRacePilotData:
					if (change is ChangeEventArgs<InjestRacePilot> racePilotArgs)
						OnInjestRaceDataChanged?.Invoke(this, racePilotArgs);
					group = ChangeGroup.InjestRacePilot.ToString();
					groupData = ChangeGroup.InjestRacePilotData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestRacePilotData;
					break;
				case ChangeGroup.InjestPilotResult:
				case ChangeGroup.InjestPilotResultData:
					if (change is ChangeEventArgs<InjestPilotResult> pilotResultArgs)
						OnInjestPilotResultChanged?.Invoke(this, pilotResultArgs);
					group = ChangeGroup.InjestPilotResult.ToString();
					groupData = ChangeGroup.InjestPilotResultData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestPoilotResultData;
					break;
				/*case ChangeGroup.InjestPosition:
				case ChangeGroup.InjestPositionData:
					if (change is ChangeEventArgs<InjestPosition> positionArgs)
						OnInjestPilotResultChanged?.Invoke(this, positionArgs);
					group = ChangeGroup.InjestPosition.ToString();
					groupData = ChangeGroup.InjestPositionData.ToString();
					dataMethod = ChangeSignalMessages.ChangeInjestPositionData;
					break;*/
			}

			await changeHub.Clients.Group(group)
                .SendAsync(ChangeSignalMessages.Change, group, change.Id, change.ParentId);
			await changeHub.Clients.Group(groupData)
				.SendAsync(dataMethod, groupData, change.Id, change.ParentId, change.Data);
		}
    }
}
