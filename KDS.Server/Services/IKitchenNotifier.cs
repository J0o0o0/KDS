using KDS.Server.DTOs;

namespace KDS.Server.Services
{
    public interface IKitchenNotifier
    {
        Task NotifyOrderCreatedAsync(OrderDto order);
        Task NotifyOrderUpdatedAsync(OrderDto order);
        Task NotifyOrderStatusChangedAsync(int orderId, string status);
        Task NotifyComponentStatusChangedAsync(int orderId, int componentId, string status);
    }
}

