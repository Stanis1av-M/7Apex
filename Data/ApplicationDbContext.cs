using Apex7.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apex7.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // 1. РЕГИСТРАЦИЯ ВСЕХ ТАБЛИЦ (Только DbSets!)
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Manufacture> Manufacturers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        // Исправил названия на множественное число:
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<FavoriteProduct> FavoriteProducts { get; set; }

        public DbSet<Order> Orders { get; set; } // Было Order, стало Orders
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<DeliveryMethod> DeliveryMethods { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        public DbSet<Tour> Tours { get; set; }
        public DbSet<TourGroup> TourGroups { get; set; }
        public DbSet<TourBooking> TourBookings { get; set; }
        public DbSet<ComplexityLevel> ComplexityLevels { get; set; }

        public DbSet<Supply> Supplies { get; set; } // Было Supply, стало Supplies
        public DbSet<SupplyItem> SupplyItems { get; set; }

        public DbSet<News> News { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // 2. НАСТРОЙКА СВЯЗЕЙ И ПРАВИЛ УДАЛЕНИЯ (Fluent API)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Автоматически ставим формат денег (decimal) для всех цен
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // ==========================================
            // БЛОК ЗАКАЗОВ И ЮЗЕРОВ
            // ==========================================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>().HasOne(o => o.OrderStatus).WithMany(s => s.Orders).HasForeignKey(o => o.OrderStatusId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>().HasOne(o => o.DeliveryMethod).WithMany(d => d.Orders).HasForeignKey(o => o.DeliveryMethodId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>().HasOne(o => o.PaymentMethod).WithMany(p => p.Orders).HasForeignKey(o => o.PaymentMethodId).OnDelete(DeleteBehavior.Restrict);

            // ==========================================
            // БЛОК ТУРИЗМА
            // ==========================================
            modelBuilder.Entity<TourGroup>()
                .HasOne(tg => tg.Guide)
                .WithMany(u => u.GuidedGroups)
                .HasForeignKey(tg => tg.GuideId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TourBooking>()
                .HasOne(tb => tb.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(tb => tb.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==========================================
            // БЛОК СКЛАДА (Поставки)
            // ==========================================
            modelBuilder.Entity<Supply>()
                .HasOne(s => s.User)
                .WithMany(u => u.Supplies)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SupplyItem>()
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================
            // ПРОЧИЕ СВЯЗИ
            // ==========================================
            modelBuilder.Entity<ProductReview>().HasOne(pr => pr.User).WithMany(u => u.ProductReviews).HasForeignKey(pr => pr.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FavoriteProduct>().HasOne(fp => fp.User).WithMany(u => u.Favorites).HasForeignKey(fp => fp.UserId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FavoriteProduct>().HasOne(fp => fp.Product).WithMany().HasForeignKey(fp => fp.ProductId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}