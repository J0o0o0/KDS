using System.ComponentModel.DataAnnotations;

namespace KDS.Server.DTOs
{
    public class StationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
    public class CreateStationDto
    {
        [Required, StringLength(50)]
        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}

