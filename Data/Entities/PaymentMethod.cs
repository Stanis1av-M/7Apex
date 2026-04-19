namespace Apex7.Data.Entities
{
    public class PaymentMethod
    {
        public int PaymentMethodId { get; set; } // PK
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; } = true;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
