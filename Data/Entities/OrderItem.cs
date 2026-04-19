using System.ComponentModel.DataAnnotations.Schema;

namespace Apex7.Data.Entities
{
    public class OrderItem
    {
        public int OrderItemId { get; set; } // PK

        public int OrderId { get; set; } // FK
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; } // FK
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } 
    }
}
