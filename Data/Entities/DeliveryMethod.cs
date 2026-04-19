using System.ComponentModel.DataAnnotations;

namespace Apex7.Data.Entities
{
    public class DeliveryMethod
    {
        [Key]
        public int DeliveryMethodId { get; set; }

        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}