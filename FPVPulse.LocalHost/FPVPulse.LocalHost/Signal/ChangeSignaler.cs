using Microsoft.AspNetCore.SignalR;
using FPVPulse.LocalHost.Client;

namespace FPVPulse.LocalHost.Signal
{
    public class ChangeSignaler
    {
        // TODO : add additional channels where the whole data is transmitted

        public event EventHandler<ChangeEventArgs>? OnChange;
        public event EventHandler<ChangeEventArgs>? OnInjestEventChanged;
        public event EventHandler<ChangeEventArgs>? OnInjestRaceChanged;
        public event EventHandler<ChangeEventArgs>? OnInjestPilotResultChanged;

        readonly IHubContext<ChangeHub> changeHub;

        public ChangeSignaler(IHubContext<ChangeHub> changeHub)
        {
            this.changeHub = changeHub;
        }

        public async Task SignalChangeAsync(ChangeGroup group, int id, int parentId)
        {
            var change = new ChangeEventArgs(group, id, parentId);
            await SignalChangeAsync(change);
        }

        public async Task SignalChangeAsync(ChangeEventArgs change)
        {
            OnChange?.Invoke(this, change);
            switch(change.Group)
            {
                case ChangeGroup.InjestEvent:
                    OnInjestEventChanged?.Invoke(this, change);
                    break;
                case ChangeGroup.InjestRace:
                    OnInjestRaceChanged?.Invoke(this, change);
                    break;
                case ChangeGroup.InjestPilotResult:
                    OnInjestPilotResultChanged?.Invoke(this, change);
                    break;
            }

            await changeHub.Clients.Group(change.Group.ToString())
                .SendAsync(ChangeSignalMessages.Change, change.Group, change.Id, change.ParentId);
        }
    }
}
