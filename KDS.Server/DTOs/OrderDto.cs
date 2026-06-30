namespace KDS.Server.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int OrderNumber { get; set; }
        public string OrderType { get; set; } = string.Empty;
        public string? TableNumber { get; set; }
        public string? CustomerName { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? ServedAt { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }
    public class OrderItemDto
    {
        public int Id { get; set; }
        public string MenuItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemComponentDto> Components { get; set; } = new();
    }
    public class OrderItemComponentDto
    {
        public int Id { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string StationName { get; set; } = string.Empty;
        public int StationId { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemComponentAddOnDto> AddOns { get; set; } = new();
    }
    public class OrderItemComponentAddOnDto
    {
        public string AddOnName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public bool IsRemoval { get; set; }
    }

}
