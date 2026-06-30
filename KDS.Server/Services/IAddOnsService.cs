using KDS.Server.DTOs;

namespace KDS.Server.Services
{
    public interface IAddOnsService
    {
        Task<IEnumerable<AddOnDto>> GetAllAsync(bool activeOnly);
        Task<AddOnDto> CreateAsync(CreateAddOnDto dto);
        Task<bool> ToggleActiveAsync(int id);
    }
}
