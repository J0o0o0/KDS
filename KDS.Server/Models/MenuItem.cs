namespace KDS.Server.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public string Category { get; set; } = string.Empty;
        public int PrepTimeMinutes { get; set; } = 10;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<MenuItemComponent> Components { get; set; } = new List<MenuItemComponent>();
    }
}

