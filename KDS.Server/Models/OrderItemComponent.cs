namespace KDS.Server.Models
{
    public class OrderItemComponent
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }

        // Snapshots
        public int ComponentId { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public int? VariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public int Quantity { get; set; }          // how many pieces with this exact config
        public int StationId { get; set; }         // snapshot — where this goes
        public string StationName { get; set; } = string.Empty;
        public ItemStatus Status { get; set; } = ItemStatus.New;

        public ICollection<OrderItemComponentAddOn> AddOns { get; set; } = new List<OrderItemComponentAddOn>();
    }
}

