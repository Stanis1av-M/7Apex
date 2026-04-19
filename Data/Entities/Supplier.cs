namespace Apex7.Data.Entities
{
    public class Supplier
    {
        public int SupplierId { get; set; } // PK
        public string Name { get; set; } = null!;
        public string? ContactInfo { get; set; }
        public string? Address { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
