using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apex7.Data.Entities
{
    public class SupplyItem
    {
        [Key]
        public int SupplyItemId { get; set; } // Первичный ключ

        // ==========================================
        // ВНЕШНИЕ КЛЮЧИ (FK)
        // ==========================================

        [Required]
        public int SupplyId { get; set; }

        [ForeignKey("SupplyId")]
        public virtual Supply Supply { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        // ==========================================
        // ПАРАМЕТРЫ ПОЗИЦИИ
        // ==========================================

        [Required]
        public int Quantity { get; set; } // Количество пришедшего товара

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Цена закупки (может отличаться от цены продажи)
    }
}