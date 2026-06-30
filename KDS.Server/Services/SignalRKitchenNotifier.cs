using KDS.Server.DTOs;
using KDS.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace KDS.Server.Services
{
    public class SignalRKitchenNotifier : IKitchenNotifier
    {
        private readonly IHubContext<KitchenHub> _hub;

        public SignalRKitchenNotifier(IHubContext<KitchenHub> hub)
        {
            _hub = hub;
        }

        public Task NotifyOrderCreatedAsync(OrderDto order)
            => _hub.Clients.Group("all-orders").SendAsync("OrderCreated", order);

        public Task NotifyOrderUpdatedAsync(OrderDto order)
            => _hub.Clients.Group("all-orders").SendAsync("OrderUpdated", order);

        public Task NotifyOrderStatusChangedAsync(int orderId, string status)
            => _hub.Clients.Group("all-orders").SendAsync("OrderStatusChanged", new { orderId, status });

        public Task NotifyComponentStatusChangedAsync(int orderId, int componentId, string status)
            => _hub.Clients.Group("all-orders").SendAsync("ComponentStatusChanged", new { orderId, componentId, status });
    }
}
