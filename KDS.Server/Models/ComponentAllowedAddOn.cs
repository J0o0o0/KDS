namespace KDS.Server.Models
{
    public class ComponentAllowedAddOn
    {
        public int Id { get; set; }
        public int ComponentId { get; set; }
        public int AddOnId { get; set; }
        public int MaxQuantity { get; set; } = 1;          // e.g., max 3 Extra Cheese

        public Component? Component { get; set; }
        public AddOn? AddOn { get; set; }
    }
}
