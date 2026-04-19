using Apex7.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apex7.Data.Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        // Внешние ключи (FK)
        [Required]
        public int ManufacturerId { get; set; }
        [ForeignKey("ManufacturerId")]
        public Manufacture Manufacturer { get; set; } = null!;

        [Required]
        public int SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; } = null!;

        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;

        // Основные поля
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Article { get; set; } = null!;

        public string? ImageUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        public int Discount { get; set; } // Процент скидки

        public int Stock { get; set; } // Текущий остаток

       
        public string? AvailabilityStatus { get; set; }

        public string? Description { get; set; }

        public bool IsVisible { get; set; } = true;

        public DateTime CreationDate { get; set; } = DateTime.Now;

        public string? Tags { get; set; } // Теги через запятую или JSON

        public DateTime CreatedAt { get; set; } = DateTime.Now;

      
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    }
}