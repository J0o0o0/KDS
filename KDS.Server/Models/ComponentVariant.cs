namespace KDS.Server.Models
{
    public class ComponentVariant
    {
        public int Id { get; set; }
        public int ComponentId { get; set; }
        public string Name { get; set; } = string.Empty;   // "Small", "Medium", "Spicy", "1.5L Bottle"
        public decimal PriceDelta { get; set; } = 0;        // +$1 for Medium Fries, +$0 for Spicy
        public bool IsDefault { get; set; } = false;
        public int? OverrideStationId { get; set; }         // rare: if a variant goes to a different station
        public bool IsActive { get; set; } = true;

        public Component? Component { get; set; }
        public Station? OverrideStation { get; set; }
    }
}
