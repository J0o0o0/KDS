using KDS.Server.Data;
using KDS.Server.DTOs;
using KDS.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace KDS.Server.Services
{
    public class ComponentsService : IComponentsService
    {
        private readonly AppDbContext _dbContext;
        public ComponentsService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ComponentDto>> GetAllAsync(bool activeOnly)
        {
            var components = await _dbContext.Components
                .Include(c => c.DefaultStation)
                .Include(c => c.Variants)
                .Include(c => c.AllowedAddOns).ThenInclude(a => a.AddOn)
                .Where(c => !activeOnly || c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Load swap pairs (both directions)
            var allSwapPairs = await _dbContext.SwapPairs
                .Include(s => s.ComponentA)
                .Include(s => s.ComponentB)
                .ToListAsync();

            var result = components.Select(c =>
            {
                var swaps = allSwapPairs
                    .Where(s => s.ComponentAId == c.Id || s.ComponentBId == c.Id)
                    .Select(s => new SwapOptionDto
                    {
                        ComponentId = s.ComponentAId == c.Id ? s.ComponentBId : s.ComponentAId,
                        ComponentName = s.ComponentAId == c.Id ? s.ComponentB!.Name : s.ComponentA!.Name
                    }).ToList();

                return new ComponentDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DefaultStationId = c.DefaultStationId,
                    DefaultStationName = c.DefaultStation!.Name,
                    IsActive = c.IsActive,
                    Variants = c.Variants.OrderBy(v => v.Name).Select(v => new ComponentVariantDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                        PriceDelta = v.PriceDelta,
                        IsDefault = v.IsDefault,
                        IsActive = v.IsActive
                    }).ToList(),
                    AllowedAddOns = c.AllowedAddOns.Select(a => new AllowedAddOnDto
                    {
                        AddOnId = a.AddOnId,
                        AddOnName = a.AddOn!.Name,
                        MaxQuantity = a.MaxQuantity
                    }).ToList(),
                    SwappableWith = swaps
                };
            }).ToList();

            return result;
        }

        public async Task<ComponentDto> CreateAsync(CreateComponentDto dto)
        {
            if (!await _dbContext.Stations.AnyAsync(s => s.Id == dto.DefaultStationId))
                throw new ArgumentException("Invalid StationId");

            var component = new Component
            {
                Name = dto.Name,
                Description = dto.Description,
                DefaultStationId = dto.DefaultStationId,
                IsActive = dto.IsActive
            };

            // Add variants inline
            foreach (var v in dto.Variants)
            {
                component.Variants.Add(new ComponentVariant
                {
                    Name = v.Name,
                    PriceDelta = v.PriceDelta,
                    IsDefault = v.IsDefault,
                    IsActive = true
                });
            }

            // Link addons
            foreach (var a in dto.AddOns)
            {
                component.AllowedAddOns.Add(new ComponentAllowedAddOn
                {
                    AddOnId = a.AddOnId,
                    MaxQuantity = a.MaxQuantity
                });
            }

            _dbContext.Components.Add(component);
            await _dbContext.SaveChangesAsync();

            // Create swap pairs (store with smaller ID first to prevent duplicates)
            foreach (var otherId in dto.SwappableWithIds)
            {
                if (otherId == component.Id) continue;
                var (a, b) = component.Id < otherId ? (component.Id, otherId) : (otherId, component.Id);

                // Check if pair already exists
                var exists = await _dbContext.SwapPairs.AnyAsync(s => s.ComponentAId == a && s.ComponentBId == b);
                if (!exists)
                {
                    _dbContext.SwapPairs.Add(new SwapPair { ComponentAId = a, ComponentBId = b });
                }
            }
            await _dbContext.SaveChangesAsync();

            return new ComponentDto
            {
                Id = component.Id,
                Name = component.Name,
                Description = component.Description,
                DefaultStationId = component.DefaultStationId,
                IsActive = component.IsActive
            };
        }

        public async Task<bool> UpdateAsync(int id, CreateComponentDto dto)
        {
            var component = await _dbContext.Components
                .Include(c => c.Variants)
                .Include(c => c.AllowedAddOns)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (component is null) return false;

            if (!await _dbContext.Stations.AnyAsync(s => s.Id == dto.DefaultStationId))
                throw new ArgumentException("Invalid StationId");

            // ---- Scalar fields ----
            component.Name = dto.Name;
            component.Description = dto.Description;
            component.DefaultStationId = dto.DefaultStationId;
            component.IsActive = dto.IsActive;

            // ---- Variants: replace-all ----
            _dbContext.ComponentVariants.RemoveRange(component.Variants);
            component.Variants.Clear();
            foreach (var v in dto.Variants)
            {
                component.Variants.Add(new ComponentVariant
                {
                    Name = v.Name,
                    PriceDelta = v.PriceDelta,
                    IsDefault = v.IsDefault,
                    IsActive = true
                });
            }

            // ---- Allowed add-ons: replace-all ----
            _dbContext.ComponentAllowedAddOns.RemoveRange(component.AllowedAddOns);
            component.AllowedAddOns.Clear();
            foreach (var a in dto.AddOns)
            {
                component.AllowedAddOns.Add(new ComponentAllowedAddOn
                {
                    AddOnId = a.AddOnId,
                    MaxQuantity = a.MaxQuantity
                });
            }

            await _dbContext.SaveChangesAsync();

            // ---- Swap pairs: replace-all ----
            // SwapPairs always stores the smaller id first (see CreateAsync/AddSwapAsync),
            // so normalize the same way here before comparing/removing/adding.
            var existingPairs = await _dbContext.SwapPairs
                .Where(s => s.ComponentAId == id || s.ComponentBId == id)
                .ToListAsync();

            var desiredOtherIds = dto.SwappableWithIds.Where(otherId => otherId != id).ToHashSet();

            // Remove pairs that are no longer wanted.
            var pairsToRemove = existingPairs
                .Where(s =>
                {
                    var otherId = s.ComponentAId == id ? s.ComponentBId : s.ComponentAId;
                    return !desiredOtherIds.Contains(otherId);
                })
                .ToList();
            _dbContext.SwapPairs.RemoveRange(pairsToRemove);

            // Add pairs that are newly wanted (skip ones that already exist).
            var existingOtherIds = existingPairs
                .Select(s => s.ComponentAId == id ? s.ComponentBId : s.ComponentAId)
                .ToHashSet();

            foreach (var otherId in desiredOtherIds)
            {
                if (existingOtherIds.Contains(otherId)) continue; // already linked, keep as-is

                var (a, b) = id < otherId ? (id, otherId) : (otherId, id);
                var alreadyExists = await _dbContext.SwapPairs.AnyAsync(s => s.ComponentAId == a && s.ComponentBId == b);
                if (!alreadyExists)
                {
                    _dbContext.SwapPairs.Add(new SwapPair { ComponentAId = a, ComponentBId = b });
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }


        public async Task<bool> AddVariantAsync(int id, CreateVariantDto dto)
        {
            var component = await _dbContext.Components.FindAsync(id);
            if (component is null) return false;

            _dbContext.ComponentVariants.Add(new ComponentVariant
            {
                ComponentId = id,
                Name = dto.Name,
                PriceDelta = dto.PriceDelta,
                IsDefault = dto.IsDefault,
                IsActive = true
            });
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddAllowedAddOnAsync(int id, AllowedAddOnDto dto)
        {
            var component = await _dbContext.Components.FindAsync(id);
            if (component is null) return false;
            if (!await _dbContext.AddOns.AnyAsync(a => a.Id == dto.AddOnId)) return false;

            _dbContext.ComponentAllowedAddOns.Add(new ComponentAllowedAddOn
            {
                ComponentId = id,
                AddOnId = dto.AddOnId,
                MaxQuantity = dto.MaxQuantity
            });
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddSwapAsync(int componentAId, int componentBId)
        {
            if (componentAId == componentBId) return false;
            if (!await _dbContext.Components.AnyAsync(c => c.Id == componentAId)) return false;
            if (!await _dbContext.Components.AnyAsync(c => c.Id == componentBId)) return false;

            var (a, b) = componentAId < componentBId ? (componentAId, componentBId) : (componentBId, componentAId);
            var exists = await _dbContext.SwapPairs.AnyAsync(s => s.ComponentAId == a && s.ComponentBId == b);
            if (exists) return true;

            _dbContext.SwapPairs.Add(new SwapPair { ComponentAId = a, ComponentBId = b });
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}