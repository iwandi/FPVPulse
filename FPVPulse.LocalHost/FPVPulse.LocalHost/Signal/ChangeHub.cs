using Microsoft.AspNetCore.SignalR;

namespace FPVPulse.LocalHost.Signal
{
    public class ChangeHub : Hub
    {
        public async Task Subscribe(ChangeGroup group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group.ToString());
        }

        public async Task Unsubscribe(ChangeGroup group)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group.ToString());
        }
    }
}
