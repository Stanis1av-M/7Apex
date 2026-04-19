using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Apex7.Data.Entities
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string Email { get; set; } = null!;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public string? Status { get; set; }
        public string Password { get; set; } = null!;
        public string? Description { get; set; }
        public bool Ban { get; set; } = false;
        public string? PhotoPath { get; set; }

        // ЭТИ СВОЙСТВА ОБЯЗАТЕЛЬНЫ, ЧТОБЫ УБРАТЬ ОШИБКИ:

        // Связь с Заказами
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        // Связь с Группами туров (для гидов)
        public ICollection<TourGroup> GuidedGroups { get; set; } = new List<TourGroup>();

        // Связь с Бронированиями (для клиентов)
        public ICollection<TourBooking> Bookings { get; set; } = new List<TourBooking>();

        // Связь с Поставками (для менеджеров)
        public ICollection<Supply> Supplies { get; set; } = new List<Supply>();

        // Связь с Отзывами
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

        // Связь с Избранным
        public ICollection<FavoriteProduct> Favorites { get; set; } = new List<FavoriteProduct>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}