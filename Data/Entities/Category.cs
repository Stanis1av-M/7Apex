namespace Apex7.Data.Entities
{
    public class Category
    {
        public int CategoryId { get; set; } // PK
        public string Name { get; set; } = null!;

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
