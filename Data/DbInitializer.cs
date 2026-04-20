using Apex7.Data.Entities;
using System;
using System.Linq;

namespace Apex7.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // 1. РОЛИ
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Администратор" },
                    new Role { Name = "Менеджер" },
                    new Role { Name = "Гид" },
                    new Role { Name = "Клиент" }
                );
                context.SaveChanges();
            }

            
            // 2. ПОЛЬЗОВАТЕЛИ
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.First(r => r.Name == "Администратор").RoleId;
                var guideRole = context.Roles.First(r => r.Name == "Гид").RoleId;
                var managerRole = context.Roles.First(r => r.Name == "Менеджер").RoleId; // Находим роль Менеджера

                context.Users.AddRange(
                    new User
                    {
                        FullName = "Иван Админов",
                        Email = "admin@world.com",
                        Password = "123",
                        RoleId = adminRole,
                        RegistrationDate = DateTime.Now
                    },
                    new User
                    {
                        FullName = "Александр Котляр",
                        Email = "guide@world.com",
                        Password = "123",
                        RoleId = guideRole,
                        PhotoPath = "/images/guides/alex.jpg",
                        Description = "Профессиональный альпинист, 10 лет опыта.",
                        RegistrationDate = DateTime.Now
                    },
                    // ДОБАВЛЯЕМ МЕНЕДЖЕРА:
                    new User
                    {
                        FullName = "Марина Менеджерова",
                        Email = "manager@world.com",
                        Password = "123",
                        RoleId = managerRole,
                        RegistrationDate = DateTime.Now
                    }
                );
                context.SaveChanges();
            }

            // 3. КАТЕГОРИИ И БРЕНДЫ 

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Name = "Палатки" },
                    new Category { Name = "Рюкзаки" },
                    new Category { Name = "Спальники" },
                    new Category { Name = "Оборудование" }
                );
                context.Manufacturers.AddRange(
                    new Manufacture { Name = "North Face", Country = "USA" },
                    new Manufacture { Name = "Osprey", Country = "USA" },
                    new Manufacture { Name = "Petzl", Country = "France" },
                    new Manufacture { Name = "Mammut", Country = "Switzerland" }
                );
                context.Suppliers.Add(new Supplier { Name = "АльпИндустрия", ContactInfo = "info@alp.ru" });
                context.SaveChanges();
            }

            // 4. ТОВАРЫ (по несколько штук на категорию)
            if (!context.Products.Any())
            {
                var cats = context.Categories.ToList();
                var mans = context.Manufacturers.ToList();
                var supId = context.Suppliers.First().SupplierId;

                context.Products.AddRange(
                    // ПАЛАТКИ
                    new Product
                    {
                        Name = "Экспедиционная палатка Storm 3",
                        Article = "TENT-001",
                        Price = 15000,
                        OldPrice = 18000,
                        Discount = 15,
                        Stock = 5,
                        CategoryId = cats[0].CategoryId,
                        ManufacturerId = mans[0].ManufactureId,
                        SupplierId = supId,
                        ImageUrl = "/images/products/palatka.png",
                        IsVisible = true,
                        CreatedAt = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Палатка Solo Ultralight",
                        Article = "TENT-002",
                        Price = 8500,
                        Stock = 3,
                        CategoryId = cats[0].CategoryId,
                        ManufacturerId = mans[3].ManufactureId,
                        SupplierId = supId,
                        ImageUrl = "/images/products/palatka1.png",
                        IsVisible = true,
                        CreatedAt = DateTime.Now
                    },
                    // РЮКЗАКИ
                    new Product
                    {
                        Name = "Рюкзак Osprey Aether 65",
                        Article = "BACK-001",
                        Price = 24000,
                        OldPrice = 28000,
                        Discount = 14,
                        Stock = 10,
                        CategoryId = cats[1].CategoryId,
                        ManufacturerId = mans[1].ManufactureId,
                        SupplierId = supId,
                        ImageUrl = "/images/products/back.png",
                        IsVisible = true,
                        CreatedAt = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Гермомешок 10л",
                        Article = "BACK-ACC",
                        Price = 950,
                        Stock = 40, // Меньше 1000 - обычный вид
                        CategoryId = cats[1].CategoryId,
                        ManufacturerId = mans[1].ManufactureId,
                        SupplierId = supId,
                        ImageUrl = "/images/products/drybag.png",
                        IsVisible = true,
                        CreatedAt = DateTime.Now
                    },
                    // СПАЛЬНИКИ
                    new Product
                    {
                        Name = "Спальник Arctic Comfort",
                        Article = "SLEEP-001",
                        Price = 12000,
                        OldPrice = 14500,
                        Discount = 17,
                        Stock = 8,
                        CategoryId = cats[2].CategoryId,
                        ManufacturerId = mans[3].ManufactureId,
                        SupplierId = supId,
                        ImageUrl = "/images/products/sleep.png",
                        IsVisible = true,
                        CreatedAt = DateTime.Now
                    },
                    // ОБОРУДОВАНИЕ (ОБЫЧНЫЕ ТОВАРЫ)
                    new Product
                    {
                        Name = "Налобный фонарь Petzl Swift",
                        Article = "GEAR-001",
                        Price = 7200,
                        Stock = 15,
                        CategoryId = cats[3].CategoryId,
                        ManufacturerId = mans[2].ManufactureId,
                        SupplierId = supId,
                        ImageUrl = "/images/products/lamp.png",
                        IsVisible = true,
                        CreatedAt = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Карабин Climbing Pro",
                        Article = "GEAR-002",
                        Price = 450,
                        Stock = 100, // Меньше 1000
                        CategoryId = cats[3].CategoryId,
                        ManufacturerId = mans[2].ManufactureId,
                        SupplierId = supId,
                        ImageUrl = "/images/products/carabiner.png",
                        IsVisible = true,
                        CreatedAt = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Газовая горелка PocketRocket",
                        Article = "GEAR-003",
                        Price = 3200,
                        OldPrice = 4000,
                        Discount = 20,
                        Stock = 12,
                        CategoryId = cats[3].CategoryId,
                        ManufacturerId = mans[0].ManufactureId,
                        SupplierId = supId,
                        ImageUrl = "/images/products/stove.png",
                        IsVisible = true,
                        CreatedAt = DateTime.Now
                    }
                );
                context.SaveChanges();
            }



            // 5. ТУРЫ И СЛОЖНОСТЬ
          
            if (!context.ComplexityLevels.Any())
            {
                context.ComplexityLevels.AddRange(
                    new ComplexityLevel { Name = "Легкий" },
                    new ComplexityLevel { Name = "Средний" },
                    new ComplexityLevel { Name = "Сложный" }
                );

                context.SaveChanges();
            }

            
            var easy = context.ComplexityLevels.First(c => c.Name == "Легкий").ComplexityLevelId;
            var medium = context.ComplexityLevels.First(c => c.Name == "Средний").ComplexityLevelId;
            var hard = context.ComplexityLevels.First(c => c.Name == "Сложный").ComplexityLevelId;


           
            if (!context.Tours.Any())
            {
                context.Tours.AddRange(
                    new Tour { Name = "Килиманджаро", Region = "Африка", DurationDays = 8, ComplexityLevelId = hard, ImageUrl = "/images/news/kilim.jpg", Description = "Восхождение на высочайшую точку Африки через тропические леса к ледникам." },
                    new Tour { Name = "Базовый лагерь Эвереста", Region = "Непал", DurationDays = 14, ComplexityLevelId = hard, ImageUrl = "/images/tours/everest.png", Description = "Легендарный трек к подножию высочайшей горы мира через культуру шерпов." },
                    new Tour { Name = "Монблан", Region = "Альпы", DurationDays = 7, ComplexityLevelId = hard, ImageUrl = "/images/tours/apext.png", Description = "Классика альпинизма: восхождение на главную вершину Западной Европы." },
                    new Tour { Name = "Патагония: W-трек", Region = "Чили", DurationDays = 5, ComplexityLevelId = medium, ImageUrl = "/images/tours/w.png", Description = "Бирюзовые озера, гранитные башни и гигантские ледники на краю земли." },
                    new Tour { Name = "Исландия: Скрытые земли", Region = "Исландия", DurationDays = 6, ComplexityLevelId = medium, ImageUrl = "/images/tours/iceland.png", Description = "Разноцветные горы Ландманналаугар и черные пустыни вулканического острова." },
                    new Tour { Name = "Лофотенские острова", Region = "Норвегия", DurationDays = 7, ComplexityLevelId = easy, ImageUrl = "/images/tours/lofoten.png", Description = "Живописные рыбацкие деревни и крутые скалы, вырастающие прямо из океана." },
                    new Tour { Name = "Долина гейзеров", Region = "Камчатка", DurationDays = 10, ComplexityLevelId = medium, ImageUrl = "/images/tours/kamchatka.png", Description = "Мир действующих вулканов, горячих источников и дикой природы Тихого океана." }
                );

                context.SaveChanges();
            }
            // 6. НОВОСТИ (Экспедиции)
            if (!context.News.Any())
            {
                context.News.AddRange(
                    new News
                    {
                        Title = "КИЛИМАНДЖАРО: ВЕРШИНА АФРИКИ ПОКОРЕНА",
                        Content = "Группа из 12 участников под руководством гидов 7apex успешно достигла пика Ухуру. Погода была суровой, но командный дух позволил всем взойти на вершину на рассвете.",
                        ImageURL = "/images/news/kilim.jpg",
                        CreatedAt = DateTime.Now.AddDays(-5),
                        IsArchived = false
                    },
                    new News
                    {
                        Title = "МАНАСЛУ: ГИМАЛАЙСКИЙ ТРИУМФ",
                        Content = "Сложнейшее восхождение на восьмитысячник завершено. Наша команда установила новый рекорд скорости подъема для этого сезона. Все участники уже в базовом лагере.",
                        ImageURL = "/images/news/apex.png",
                        CreatedAt = DateTime.Now.AddDays(-10),
                        IsArchived = false
                    },
                    new News
                    {
                        Title = "ЭЛЬБРУС: СЕЗОН В САМОМ РАЗГАРЕ",
                        Content = "Завершилась первая летняя смена на высочайшей точке Европы. 100% группы достигли западной вершины. Мы открываем набор на август!",
                        ImageURL = "/images/news/elbrus.png",
                        CreatedAt = DateTime.Now.AddDays(-2),
                        IsArchived = false
                    }
                );
                context.SaveChanges();
            }
            // 7. МЕТОДЫ ОПЛАТЫ И ДОСТАВКИ (Убедись, что они есть!)
            if (!context.PaymentMethods.Any())
            {
                context.PaymentMethods.AddRange(
                    new PaymentMethod { Name = "Картой онлайн", IsActive = true },
                    new PaymentMethod { Name = "Наличными", IsActive = true }
                );
                context.SaveChanges();
            }

            if (!context.DeliveryMethods.Any())
            {
                context.DeliveryMethods.AddRange(
                    new DeliveryMethod { Name = "Самовывоз", Price = 0, IsActive = true },
                    new DeliveryMethod { Name = "Курьерская доставка", Price = 500, IsActive = true }
                );
                context.SaveChanges();
            }
        }
    }
}