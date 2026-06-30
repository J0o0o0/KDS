namespace KDS.Server.Models
{
    public class Component
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DefaultStationId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Station? DefaultStation { get; set; }
        public ICollection<ComponentVariant> Variants { get; set; } = new List<ComponentVariant>();
        public ICollection<ComponentAllowedAddOn> AllowedAddOns { get; set; } = new List<ComponentAllowedAddOn>();
    }
}
