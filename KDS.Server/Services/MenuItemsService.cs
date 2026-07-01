using KDS.Server.Data;
using KDS.Server.DTOs;
using KDS.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace KDS.Server.Services
{
    public class MenuItemsService : IMenuItemsService
    {
        private readonly AppDbContext _dbContext;
        public MenuItemsService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<MenuItemDto>> GetAllAsync(bool activeOnly)
        {
            var query = _dbContext.MenuItems
                .Include(m => m.Components).ThenInclude(s => s.Component)
                .AsQueryable();
            if (activeOnly) query = query.Where(m => m.IsActive);

            return await query.OrderBy(m => m.Category).ThenBy(m => m.Name)
                .Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    BasePrice = m.BasePrice,
                    Category = m.Category,
                    PrepTimeMinutes = m.PrepTimeMinutes,
                    IsActive = m.IsActive,
                    Components = m.Components.Select(mc => new MenuItemComponentDto
                    {
                        ComponentId = mc.ComponentId,
                        ComponentName = mc.Component!.Name,
                        Quantity = mc.Quantity
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto)
        {
            // Validate all component IDs exist
            var componentIds = dto.Components.Select(c => c.ComponentId).Distinct().ToList();
            var existingComponents = await _dbContext.Components
                .Where(c => componentIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            if (existingComponents.Count != componentIds.Count)
                throw new ArgumentException("One or more ComponentId values are invalid");

            var menuItem = new MenuItem
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                Category = dto.Category,
                PrepTimeMinutes = dto.PrepTimeMinutes,
                IsActive = dto.IsActive
            };

            foreach (var c in dto.Components)
            {
                menuItem.Components.Add(new MenuItemComponent
                {
                    ComponentId = c.ComponentId,
                    Quantity = c.Quantity
                });
            }

            _dbContext.MenuItems.Add(menuItem);
            await _dbContext.SaveChangesAsync();

            return new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                BasePrice = menuItem.BasePrice,
                Category = menuItem.Category,
                IsActive = menuItem.IsActive
            };
        }
        public async Task<bool> UpdateAsync(int id, CreateMenuItemDto dto)
        {
            var menuItem = await _dbContext.MenuItems
                .Include(m => m.Components)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem is null) return false;

            // Validate all component IDs exist.
            var componentIds = dto.Components.Select(c => c.ComponentId).Distinct().ToList();
            var existingComponents = await _dbContext.Components
                .Where(c => componentIds.Contains(c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            if (existingComponents.Count != componentIds.Count)
                throw new ArgumentException("One or more ComponentId values are invalid");

            // ---- Scalar fields ----
            menuItem.Name = dto.Name;
            menuItem.Description = dto.Description;
            menuItem.BasePrice = dto.BasePrice;
            menuItem.Category = dto.Category;
            menuItem.PrepTimeMinutes = dto.PrepTimeMinutes;
            menuItem.IsActive = dto.IsActive;
            menuItem.UpdatedAt = DateTime.UtcNow; // same field ToggleActiveAsync already sets

            // ---- Components: replace-all ----
            _dbContext.MenuItemComponents.RemoveRange(menuItem.Components);
            menuItem.Components.Clear();
            foreach (var c in dto.Components)
            {
                menuItem.Components.Add(new MenuItemComponent
                {
                    ComponentId = c.ComponentId,
                    Quantity = c.Quantity
                });
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ToggleActiveAsync(int id)
        {
            var m = await _dbContext.MenuItems.FindAsync(id);
            if (m is null) return false;
            m.IsActive = !m.IsActive;
            m.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
