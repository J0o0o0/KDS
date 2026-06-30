using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace KDS.Server.Hubs
{
    [Authorize]
    public class KitchenHub : Hub
    {
        public Task JoinStationGroup(int stationId)
            => Groups.AddToGroupAsync(Context.ConnectionId, $"station-{stationId}");

        public Task LeaveStationGroup(int stationId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"station-{stationId}");

        public Task JoinExpediterGroup()
            => Groups.AddToGroupAsync(Context.ConnectionId, "expediter");

        public Task LeaveExpediterGroup()
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, "expediter");

        public Task JoinAllOrdersGroup()
            => Groups.AddToGroupAsync(Context.ConnectionId, "all-orders");

        public Task LeaveAllOrdersGroup()
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, "all-orders");

        public override async Task OnConnectedAsync()
        {
            // Auto-join "all-orders" group on connect (every authenticated user)
            await Groups.AddToGroupAsync(Context.ConnectionId, "all-orders");
            await base.OnConnectedAsync();
        }
    }
}
