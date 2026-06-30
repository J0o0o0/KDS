using KDS.Server.Data;
using KDS.Server.DTOs;
using KDS.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace KDS.Server.Services
{
    public class AddOnsService : IAddOnsService
    {
        private readonly AppDbContext _dbContext;
        public AddOnsService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<AddOnDto>> GetAllAsync(bool activeOnly)
        {
            var query = _dbContext.AddOns.AsQueryable();
            if (activeOnly) query = query.Where(a => a.IsActive);

            return await query.OrderBy(a => a.Name).Select(a => new AddOnDto
            {
                Id = a.Id,
                Name = a.Name,
                Price = a.Price,
                IsRemoval = a.IsRemoval,
                IsActive = a.IsActive
            }).ToListAsync();
        }

        public async Task<AddOnDto> CreateAsync(CreateAddOnDto dto)
        {
            var a = new AddOn
            {
                Name = dto.Name,
                Price = dto.Price,
                IsRemoval = dto.IsRemoval,
                IsActive = dto.IsActive
            };
            _dbContext.AddOns.Add(a);
            await _dbContext.SaveChangesAsync();
            return new AddOnDto
            {
                Id = a.Id,
                Name = a.Name,
                Price = a.Price,
                IsRemoval = a.IsRemoval,
                IsActive = a.IsActive
            };
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var a = await _dbContext.AddOns.FindAsync(id);
            if (a is null) return false;
            a.IsActive = !a.IsActive;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
