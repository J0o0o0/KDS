namespace KDS.Server.DTOs
{
    public class ComponentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DefaultStationId { get; set; }
        public string? DefaultStationName { get; set; }
        public bool IsActive { get; set; }
        public List<ComponentVariantDto> Variants { get; set; } = new();
        public List<AllowedAddOnDto> AllowedAddOns { get; set; } = new();
        public List<SwapOptionDto> SwappableWith { get; set; } = new();
    }

    public class ComponentVariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PriceDelta { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    public class AllowedAddOnDto
    {
        public int AddOnId { get; set; }
        public string AddOnName { get; set; } = string.Empty;
        public int MaxQuantity { get; set; }
    }
    public class SwapOptionDto
    {
        public int ComponentId { get; set; }
        public string ComponentName { get; set; } = string.Empty;
    }

    public class CreateComponentDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DefaultStationId { get; set; }
        public bool IsActive { get; set; } = true;
        // Admin selects applicable addons from the global catalog
        public List<ComponentAddOnLinkDto> AddOns { get; set; } = new();
        // Admin selects which components this can swap with
        public List<int> SwappableWithIds { get; set; } = new();
        // Admin creates variants inline
        public List<CreateVariantDto> Variants { get; set; } = new();
    }
    public class ComponentAddOnLinkDto
    {
        public int AddOnId { get; set; }
        public int MaxQuantity { get; set; } = 1;
    }
    public class CreateVariantDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal PriceDelta { get; set; } = 0;
        public bool IsDefault { get; set; } = false;
    }
    
}

