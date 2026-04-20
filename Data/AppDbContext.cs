using Microsoft.EntityFrameworkCore;
using ShoeStore.Models;
using System;
using System.IO;

namespace ShoeStore.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // База данных будет создана в папке с исполняемым файлом
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "shoe.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка связей
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany()
                .HasForeignKey(p => p.SupplierId);
            
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId);

            // Инициализация данными (Seed) при первом запуске
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Пользователи
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Login = "admin", Password = "admin", Role = "Admin", FullName = "Администратор Системы" },
                new User { Id = 2, Login = "manager", Password = "manager", Role = "Manager", FullName = "Иванов Иван Иванович" },
                new User { Id = 3, Login = "client", Password = "client", Role = "Client", FullName = "Петров Петр Петрович" }
            );

            // Категории
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Кроссовки" },
                new Category { Id = 2, Name = "Ботинки" },
                new Category { Id = 3, Name = "Туфли" }
            );

            // Поставщики
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { Id = 1, Name = "Nike Corp" },
                new Supplier { Id = 2, Name = "Adidas LLC" },
                new Supplier { Id = 3, Name = "Local Shoes" }
            );

            // Товары (примеры)
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Air Max", CategoryId = 1, Description = "Беговые кроссовки", Manufacturer = "Nike", SupplierId = 1, Price = 12000, Unit = "пара", Quantity = 10, Discount = 0, ImagePath = "" },
                new Product { Id = 2, Name = "Ultraboost", CategoryId = 1, Description = "Комфортный бег", Manufacturer = "Adidas", SupplierId = 2, Price = 14000, Unit = "пара", Quantity = 0, Discount = 20, ImagePath = "" }, // Нет на складе + скидка > 15%
                new Product { Id = 3, Name = "Classic Leather", CategoryId = 3, Description = "Офисный стиль", Manufacturer = "Local", SupplierId = 3, Price = 5000, Unit = "пара", Quantity = 5, Discount = 5, ImagePath = "" }
            );
            
             // Статусы заказов (для ComboBox) можно захардкодить в форме или создать таблицу, здесь просто примеры данных
             modelBuilder.Entity<Order>().HasData(
                 new Order { Id = 1, ProductId = 1, StatusCode = "Новый", Address = "ул. Ленина 1", OrderDate = DateTime.Now, DeliveryDate = null }
             );
        }
    }
}