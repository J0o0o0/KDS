using KDS.Server.DTOs;

namespace KDS.Server.Services
{
    public interface IMenuItemsService
    {
        Task<IEnumerable<MenuItemDto>> GetAllAsync(bool activeOnly);
        Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto);
        Task<bool> UpdateAsync(int id, CreateMenuItemDto dto);
        Task<bool> ToggleActiveAsync(int id);
    }
}
