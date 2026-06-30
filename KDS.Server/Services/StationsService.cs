using KDS.Server.Data;
using KDS.Server.DTOs;
using KDS.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace KDS.Server.Services
{
    public class StationsService : IStationsService
    {
        private readonly AppDbContext _dbContext;
        public StationsService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<StationDto>> GetAllAsync()
        {
            return await _dbContext.Stations
                .OrderBy(s => s.SortOrder)
                .Select(s => new StationDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Color = s.Color,
                    SortOrder = s.SortOrder,
                    IsActive = s.IsActive
                })
                .ToListAsync();
        }

        public async Task<StationDto?> GetAsync(int id)
        {
            var s = await _dbContext.Stations.FindAsync(id);
            if (s is null) return null;
            return new StationDto
            {
                Id = s.Id,
                Name = s.Name,
                Color = s.Color,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive
            };
        }

        public async Task<StationDto> CreateAsync(CreateStationDto dto)
        {
            var station = new Station
            {
                Name = dto.Name,
                Color = dto.Color ?? "#6b7280",
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive
            };
            _dbContext.Stations.Add(station);
            await _dbContext.SaveChangesAsync();
            return new StationDto
            {
                Id = station.Id,
                Name = station.Name,
                Color = station.Color,
                SortOrder = station.SortOrder,
                IsActive = station.IsActive
            };
        }

        public async Task<bool> UpdateAsync(int id, CreateStationDto dto)
        {
            var station = await _dbContext.Stations.FindAsync(id);
            if (station is null) return false;

            station.Name = dto.Name;
            station.Color = dto.Color ?? "#6b7280";
            station.SortOrder = dto.SortOrder;
            station.IsActive = dto.IsActive;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var station = await _dbContext.Stations.FindAsync(id);
            if (station is null) return false;
            station.IsActive = !station.IsActive;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}