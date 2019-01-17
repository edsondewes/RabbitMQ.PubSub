using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalrNotification.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task<string> CreateRequestId()
        {
            var groupId = Guid.NewGuid().ToString();

            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            return groupId;
        }
    }
}
