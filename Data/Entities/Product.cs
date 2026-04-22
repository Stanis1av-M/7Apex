using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apex7.Data.Entities
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать производителя")]
        public int ManufacturerId { get; set; }
        [ForeignKey("ManufacturerId")]
        public virtual Manufacture? Manufacturer { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать поставщика")]
        public int SupplierId { get; set; }
        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать категорию")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [Required(ErrorMessage = "Введите название товара")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 100 символов")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Введите артикул")]
        [StringLength(30, ErrorMessage = "Артикул не может быть длиннее 30 символов")]
        public string Article { get; set; } = null!;

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Укажите цену")]
        [Range(0, 1000000, ErrorMessage = "Цена не может превышать 1 000 000 ₽")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(0, 1000000, ErrorMessage = "Старая цена не может превышать 1 000 000 ₽")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        [Range(0, 100, ErrorMessage = "Скидка должна быть от 0 до 100%")]
        public int Discount { get; set; }

        [Required(ErrorMessage = "Укажите остаток")]
        [Range(0, 99, ErrorMessage = "Количество должно быть от 0 до 99")]
        public int Stock { get; set; }

        public string? AvailabilityStatus { get; set; }

        [StringLength(250, ErrorMessage = "Описание не может быть длиннее 250 символов")]
        public string? Description { get; set; }

        public bool IsVisible { get; set; } = true;
        public string? Tags { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
        public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
        public virtual ICollection<SupplyItem> SupplyItems { get; set; } = new List<SupplyItem>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}