namespace KDS.Server.Models
{
    public class AddOn
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;   // "Extra Cheese", "No Tomato"
        public decimal Price { get; set; } = 0;
        public bool IsRemoval { get; set; } = false;       // true for "No Tomato", "No Onion"
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
