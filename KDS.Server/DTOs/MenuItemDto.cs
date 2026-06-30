using System.ComponentModel.DataAnnotations;

namespace KDS.Server.DTOs
{
    public class MenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public string Category { get; set; } = string.Empty;
        public int PrepTimeMinutes { get; set; }
        public bool IsActive { get; set; }
        public List<MenuItemComponentDto> Components { get; set; } = new();
    }
    public class MenuItemComponentDto
    {
        public int ComponentId { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
    public class UpdateMenuItemDto
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0, 10000)]
        public decimal Price { get; set; }

        [Required, StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Range(1, 120)]
        public int PrepTimeMinutes { get; set; }

        [Required]
        public int StationId { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public List<CreateMenuItemComponentDto> Components { get; set; } = new();
    }


    public class CreateMenuItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public string Category { get; set; } = string.Empty;
        public int PrepTimeMinutes { get; set; } = 10;
        public bool IsActive { get; set; } = true;
        // Admin selects existing components + quantities
        public List<CreateMenuItemComponentDto> Components { get; set; } = new();
    }
    public class CreateMenuItemComponentDto
    {
        public int ComponentId { get; set; }
        public int Quantity { get; set; } = 1;
    }

}