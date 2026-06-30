namespace KDS.Server.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Snapshot of menu item at order time
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }     // base price + variant deltas (per 1 menu item)
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
        public ItemStatus Status { get; set; } = ItemStatus.New;

        public ICollection<OrderItemComponent> Components { get; set; } = new List<OrderItemComponent>();
    }
    public enum ItemStatus
    {
        New = 0,
        InProgress = 1,
        Ready = 2,
        Bumped = 3
    }
}
