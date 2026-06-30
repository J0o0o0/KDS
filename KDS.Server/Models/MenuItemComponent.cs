namespace KDS.Server.Models
{
    public class MenuItemComponent
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        public int ComponentId { get; set; }
        public int Quantity { get; set; } = 1;   // multiplier: 4 for Box of 4

        public MenuItem? MenuItem { get; set; }
        public Component? Component { get; set; }
    }
}
