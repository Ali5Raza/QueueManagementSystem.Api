using Microsoft.AspNetCore.SignalR;

namespace QueueManagement.Api.Hubs
{
    public class QueueHub : Hub
    {
        public async Task JoinQueueGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveQueueGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}