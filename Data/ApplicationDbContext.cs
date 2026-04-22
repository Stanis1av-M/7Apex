using Apex7.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apex7.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Регистрация таблиц
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Manufacture> Manufacturers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<FavoriteProduct> FavoriteProducts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<DeliveryMethod> DeliveryMethods { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourGroup> TourGroups { get; set; }
        public DbSet<TourBooking> TourBookings { get; set; }
        public DbSet<ComplexityLevel> ComplexityLevels { get; set; }
        public DbSet<Supply> Supplies { get; set; }
        public DbSet<SupplyItem> SupplyItems { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка формата decimal для всех цен
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // ==========================================
            // БЛОК ЗАКАЗОВ (РЕШЕНИЕ КОНФЛИКТА ЦИКЛОВ)
            // ==========================================

            // Юзер -> Заказы: Используем Restrict. 
            // Это разрывает цикл "удаления всего" и успокаивает SQL Server.
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Заказ -> Позиции: Тут можно оставить Cascade.
            // Если мы всё же удалим заказ вручную, его позиции должны исчезнуть.
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Позиция -> Товар: Restrict.
            // Нельзя удалить товар, если он уже был продан в каком-то заказе.
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================
            // БЛОК СКЛАДА И ПОСТАВОК (ИСПРАВЛЕНО)
            // ==========================================

            // Менеджер -> Поставки: Restrict.
            // Поставка — это юридический документ, он не должен исчезать при удалении сотрудника.
            modelBuilder.Entity<Supply>()
                .HasOne(s => s.User)
                .WithMany(u => u.Supplies)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Поставка -> Позиции поставки: Cascade.
            modelBuilder.Entity<SupplyItem>()
                .HasOne(si => si.Supply)
                .WithMany(s => s.SupplyItems)
                .HasForeignKey(si => si.SupplyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Позиция поставки -> Товар: Restrict.
            // Убирает ту самую ошибку "cause cycles or multiple cascade paths".
            modelBuilder.Entity<SupplyItem>()
                .HasOne(si => si.Product)
                .WithMany(p => p.SupplyItems)
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================
            // БЛОК ТУРИЗМА
            // ==========================================

            // Юзер -> Бронирования: Restrict.
            modelBuilder.Entity<TourBooking>()
                .HasOne(tb => tb.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(tb => tb.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Гид -> Группы: Restrict.
            modelBuilder.Entity<TourGroup>()
                .HasOne(tg => tg.Guide)
                .WithMany(u => u.GuidedGroups)
                .HasForeignKey(tg => tg.GuideId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================
            // МЕЛКИЕ СВЯЗИ (Можно оставить Cascade)
            // ==========================================
            modelBuilder.Entity<ProductReview>().HasOne(pr => pr.User).WithMany(u => u.ProductReviews).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FavoriteProduct>().HasOne(fp => fp.User).WithMany(u => u.Favorites).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CartItem>().HasOne(ci => ci.User).WithMany(u => u.CartItems).OnDelete(DeleteBehavior.Cascade);

            // Справочники (Всегда Restrict)
            modelBuilder.Entity<Order>().HasOne(o => o.OrderStatus).WithMany(s => s.Orders).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>().HasOne(o => o.DeliveryMethod).WithMany(d => d.Orders).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>().HasOne(o => o.PaymentMethod).WithMany(p => p.Orders).OnDelete(DeleteBehavior.Restrict);
        }
    }
}