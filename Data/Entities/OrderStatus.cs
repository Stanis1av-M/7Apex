namespace Apex7.Data.Entities
{
    public class OrderStatus
    {
        public int OrderStatusID { get; set; } // PK
        public string Name { get; set; } = null!;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
