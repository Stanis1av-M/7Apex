namespace Apex7.Data.Entities
{
    public class FavoriteProduct
    {
        public int FavoriteProductId { get; set; } // PK

        public int UserId { get; set; } // FK
        public User User { get; set; } = null!;

        public int ProductId { get; set; } // FK
        public Product Product { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}
