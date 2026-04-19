namespace Apex7.Data.Entities
{
    public class Manufacture
    {
        public int ManufactureId { get; set; } // PK
        public string Name { get; set; } = null!;
        public string? Country { get; set; }

        // Навигационное свойство: у одного производителя много товаров
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
