namespace KDS.Server.Models
{
    public class OrderItemComponentAddOn
    {
        public int Id { get; set; }
        public int OrderItemComponentId { get; set; }
        public OrderItemComponent? OrderItemComponent { get; set; }

        // Snapshots
        public int AddOnId { get; set; }
        public string AddOnName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }          // total quantity (already multiplied by piece count)
        public bool IsRemoval { get; set; }
    }
}
