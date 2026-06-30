namespace KDS.Server.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;       // Grill, Fry, Salad, Drinks
        public string? Color { get; set; }                     // hex color for UI badges
        public int SortOrder { get; set; }                     // display order
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<AppUser> AssignedCooks { get; set; } = new List<AppUser>();
    }
}
