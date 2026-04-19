namespace Apex7.Data.Entities
{
    public class ProductReview
    {
        public int ProductReviewId { get; set; } // PK

        public int ProductId { get; set; } // FK
        public Product Product { get; set; } = null!;

        // Добавь эти две строки внутрь класса ProductReview:

        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string ReviewText { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Рекомендация из ТЗ: добавь это поле для админки
        public bool IsApproved { get; set; } = false;
    }
}
