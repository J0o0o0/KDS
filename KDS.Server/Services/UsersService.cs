using KDS.Server.Data;
using KDS.Server.DTOs;
using KDS.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KDS.Server.Services
{
    public class UsersService : IUsersService
    {
        private static readonly string[] ValidRoles = { "Admin", "Cashier", "Cook", "Expediter" };

        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _db;

        public UsersService(UserManager<AppUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            // Include Station for StationName without a per-user extra query.
            var users = await _userManager.Users
                .Include(u => u.Station)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var result = new List<UserDto>();
            foreach (var user in users)
            {
                result.Add(await ToDtoAsync(user));
            }
            return result;
        }

        public async Task<UserDto?> GetByIdAsync(string id)
        {
            var user = await _userManager.Users
                .Include(u => u.Station)
                .FirstOrDefaultAsync(u => u.Id == id);

            return user is null ? null : await ToDtoAsync(user);
        }

        public async Task<bool> ToggleActiveAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return false;

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<(bool found, bool validRole, UserDto? user)> AddRoleAsync(string id, string role)
        {
            if (!ValidRoles.Contains(role)) return (true, false, null);

            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return (false, true, null);

            if (!await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded) return (true, false, null);
            }

            var dto = await ToDtoAsync(user, reload: true);
            return (true, true, dto);
        }

        public async Task<(bool found, UserDto? user)> RemoveRoleAsync(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return (false, null);

            if (await _userManager.IsInRoleAsync(user, role))
            {
                if(role == "Cook")
                    user.StationId = null;
                await _userManager.RemoveFromRoleAsync(user, role);
            }

            var dto = await ToDtoAsync(user, reload: true);
            return (true, dto);
        }

        public async Task<(bool found, bool validStation, UserDto? user)> AssignStationAsync(string id, int? stationId)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return (false, true, null);

            if (stationId is not null)
            {
                var stationExists = await _db.Stations.AnyAsync(s => s.Id == stationId);
                if (!stationExists) return (true, false, null);
            }

            user.StationId = stationId;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return (true, false, null);

            var dto = await ToDtoAsync(user, reload: true);
            return (true, true, dto);
        }

        private async Task<UserDto> ToDtoAsync(AppUser user, bool reload = false)
        {
            if (reload)
            {
                await _db.Entry(user).Reference(u => u.Station).LoadAsync();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList(),
                StationId = user.StationId,
                StationName = user.Station?.Name,
            };
        }
    }
}
