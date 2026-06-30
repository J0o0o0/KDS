namespace KDS.Server.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int OrderNumber { get; set; }          // human-readable (1001, 1002...)
        public OrderType OrderType { get; set; }
        public string? TableNumber { get; set; }       // DineIn
        public string? CustomerName { get; set; }      // Pickup
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadyAt { get; set; }
        public DateTime? ServedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Snapshot of cashier identity
        public string CashierId { get; set; } = string.Empty;
        public string CashierName { get; set; } = string.Empty;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
    public enum OrderStatus
    {
        New = 0,
        InProgress = 1,
        Ready = 2,
        Served = 3,
        Cancelled = 4
    }
    public enum OrderType
    {
        DineIn = 1,
        Pickup = 2
    }
}
