using System.ComponentModel.DataAnnotations.Schema;

namespace Apex7.Data.Entities
{
    public class Order
    {
        public int OrderId { get; set; } // PK

        public int UserId { get; set; } // FK
        public User User { get; set; } = null!;

        public int OrderStatusId { get; set; } // FK
        public OrderStatus OrderStatus { get; set; } = null!;

        // Замени свои старые поля доставки на эти:
        public int DeliveryMethodId { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; } = null!;

        public int PaymentMethodId { get; set; } // FK
        public PaymentMethod PaymentMethod { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompletedAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
