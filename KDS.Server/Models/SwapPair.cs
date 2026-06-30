namespace KDS.Server.Models
{
    public class SwapPair
    {
        public int Id { get; set; }
        public int ComponentAId { get; set; }
        public int ComponentBId { get; set; }

        public Component? ComponentA { get; set; }
        public Component? ComponentB { get; set; }
    }
}
