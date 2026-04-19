using System.ComponentModel.DataAnnotations;

namespace Apex7.Data.Entities
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        // Кто покупает
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Что покупает
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Сколько штук
        public int Quantity { get; set; } = 1;
    }
}