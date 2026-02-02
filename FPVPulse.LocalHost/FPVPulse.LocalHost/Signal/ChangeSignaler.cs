using Microsoft.AspNetCore.SignalR;

namespace FPVPulse.LocalHost.Signal
{
    public enum ChangeGroup
    {
        None,
        InjestEvent,
        InjestRace,
        InjestPilotResult
    }

    public class ChangeEventArgs : EventArgs
    {
        public ChangeGroup Group { get; }
        public int Id { get; }

        public ChangeEventArgs(ChangeGroup group, int id)
        {
            Group = group;
            Id = id;
        }
    }

    public class ChangeSignaler
    {
        public event EventHandler<ChangeEventArgs>? OnChange;
        public event EventHandler<ChangeEventArgs>? OnInjestEventChanged;
        public event EventHandler<ChangeEventArgs>? OnInjestRaceChanged;
        public event EventHandler<ChangeEventArgs>? OnInjestPilotResultChanged;

        readonly IHubContext<ChangeHub> changeHub;

        public ChangeSignaler(IHubContext<ChangeHub> changeHub)
        {
            this.changeHub = changeHub;
        }

        public async Task SignalChangeAsync(ChangeGroup group, int id)
        {
            var change = new ChangeEventArgs(group, id);
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
                .SendAsync("Change", change.Group, change.Id);
        }
    }
}
