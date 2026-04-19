using System.ComponentModel.DataAnnotations.Schema;

namespace Apex7.Data.Entities
{
    public class SupplyItem
    {
        public int SupplyItemId { get; set; } // PK

        public int SupplyId { get; set; } // FK
        public Supply Supply { get; set; } = null!;

        public int ProductId { get; set; } // FK
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Цена закупки
    }
}
