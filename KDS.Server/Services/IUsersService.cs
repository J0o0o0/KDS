using KDS.Server.DTOs;

namespace KDS.Server.Services
{
    public interface IUsersService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(string id);
        Task<bool> ToggleActiveAsync(string id);

        // Returns null if user not found, "INVALID_ROLE" if the role string
        // isn't one of the 4 valid roles, otherwise the updated UserDto.
        Task<(bool found, bool validRole, UserDto? user)> AddRoleAsync(string id, string role);
        Task<(bool found, UserDto? user)> RemoveRoleAsync(string id, string role);

        // Returns false if the user isn't found, or if stationId is provided
        // but doesn't correspond to a real station.
        Task<(bool found, bool validStation, UserDto? user)> AssignStationAsync(string id, int? stationId);
    }
}
