using KDS.Server.DTOs;

namespace KDS.Server.Services
{
    public interface IStationsService
    {
        Task<IEnumerable<StationDto>> GetAllAsync();
        Task<StationDto?> GetAsync(int id);
        Task<StationDto> CreateAsync(CreateStationDto dto);
        Task<bool> UpdateAsync(int id, CreateStationDto dto);
        Task<bool> ToggleActiveAsync(int id);
    }
}
