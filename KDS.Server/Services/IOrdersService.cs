using KDS.Server.DTOs;

namespace KDS.Server.Services
{
    public interface IOrdersService
    {
        Task<OrderDto> CreateAsync(CreateOrderDto dto, string cashierId, string cashierName);
        Task<OrderDto?> GetByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetActiveAsync();           // not yet served
        Task<IEnumerable<OrderDto>> GetAllAsync(DateTime from, DateTime to);

        Task<IEnumerable<OrderDto>> GetByStationAsync(int stationId);
        Task<bool> UpdateComponentStatusAsync(int orderId, int componentId, string status);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    }
}
