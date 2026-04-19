namespace Apex7.Data.Entities
{
    public class PriceHistory
    {
        public int PriceHistoryId { get; set; } // PK

        public int ProductId { get; set; } // FK
        public Product Product { get; set; } = null!;

        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime ChangeAt { get; set; } = DateTime.Now;
    }
}
