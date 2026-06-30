namespace KDS.Server.DTOs
{
    public class AddOnDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsRemoval { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateAddOnDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; } = 0;
        public bool IsRemoval { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
