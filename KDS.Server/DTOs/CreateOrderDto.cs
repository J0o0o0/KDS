using System.ComponentModel.DataAnnotations;

namespace KDS.Server.DTOs
{
    public class CreateOrderDto
    {
        public string OrderType { get; set; } = "DineIn"; // DineIn | Pickup

        public string? TableNumber { get; set; }   // required if DineIn
        public string? CustomerName { get; set; }  // required if Pickup
        public string? Notes { get; set; }

        [Required]
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        [Required]
        public int MenuItemId { get; set; }

        [Range(1, 99)]
        public int Quantity { get; set; } = 1;

        public string? Notes { get; set; }

        // Component configurations — one entry per unique (component + variant + addons) combo
        [Required]
        public List<CreateComponentConfigDto> Components { get; set; } = new();
    }

    public class CreateComponentConfigDto
    {
        [Required]
        public int ComponentId { get; set; }

        public int? VariantId { get; set; }

        [Range(1, 99)]
        public int Quantity { get; set; } = 1;   // how many pieces with this exact config

        public List<CreateAddOnSelectionDto> AddOns { get; set; } = new();
    }

    public class CreateAddOnSelectionDto
    {
        [Required]
        public int AddOnId { get; set; }

        [Range(1, 10)]
        public int Quantity { get; set; } = 1;   // per piece
    }

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty; // New | InProgress | Ready | Bumped | Served | Cancelled
    }
}
