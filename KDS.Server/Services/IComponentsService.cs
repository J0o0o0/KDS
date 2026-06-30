using KDS.Server.DTOs;

namespace KDS.Server.Services
{
    public interface IComponentsService
    {
        Task<IEnumerable<ComponentDto>> GetAllAsync(bool activeOnly);
        Task<ComponentDto> CreateAsync(CreateComponentDto dto);
        Task<bool> UpdateAsync(int id, CreateComponentDto dto);
        Task<bool> AddVariantAsync(int id, CreateVariantDto dto);
        Task<bool> AddAllowedAddOnAsync(int id, AllowedAddOnDto dto);
        Task<bool> AddSwapAsync(int componentAId, int componentBId);
    }
}
