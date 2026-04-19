namespace Apex7.Data.Entities
{
    public class Supply
    {
        public int SupplyId { get; set; } // PK

        public int SupplierId { get; set; } // FK (из предыдущего блока)
        public Supplier Supplier { get; set; } = null!;

        public int UserId { get; set; } // FK (Менеджер, принявший поставку)
        public User User { get; set; } = null!;

        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? CompleteAt { get; set; }

        public ICollection<SupplyItem> SupplyItems { get; set; } = new List<SupplyItem>();
    }
}
