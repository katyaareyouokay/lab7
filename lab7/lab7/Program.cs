using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace lab7;

// Модель категории
[Table("category")]
public class Category
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public required string Name { get; set; }

    public List<Product> Products { get; set; } = new();
}

// Модель продукта
[Table("product")]
public class Product
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public required string Name { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    public required Category Category { get; set; }
}

// Контекст базы данных
public class AppDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Подключение к уже созданной базе данных
        optionsBuilder.UseNpgsql("Host=localhost;Database=productsdb;Username=nezkate;Password=trash2711");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка связей
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// Пример использования
public class Program
{
    public static void Main(string[] args)
    {
        using var context = new AppDbContext();

        // CRUD операции

        // 1. Создание
        Console.WriteLine("=== CREATE ===");
        try
        {
            var category = new Category { Name = "Electronics" };
            var product = new Product { Name = "Laptop", Price = 1200.99m, Category = category };

            context.Categories.Add(category);
            context.Products.Add(product);
            context.SaveChanges();

            Console.WriteLine("Category and product created successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during CREATE: {ex.Message}");
        }


        // 2. Чтение
        Console.WriteLine("\n=== READ ===");
        var categories = context.Categories.Include(c => c.Products).ToList();
        foreach (var category in categories)
        {
            Console.WriteLine($"Category: {category.Name}");
            foreach (var product in category.Products)
            {
                Console.WriteLine($"  Product: {product.Name} - {product.Price}");
            }
        }

        // 3. Обновление
        Console.WriteLine("\n=== UPDATE ===");
        var productToUpdate = context.Products.FirstOrDefault(p => p.Name == "Microwave");
        if (productToUpdate != null)
        {
            productToUpdate.Price = 180.50m;
            context.SaveChanges();
            Console.WriteLine("Product price updated!");
        }

        // 4. Удаление
        Console.WriteLine("\n=== DELETE ===");
        var categoryToDelete = context.Categories.Include(c => c.Products).FirstOrDefault(c => c.Name == "Appliances");
        if (categoryToDelete != null)
        {
            context.Categories.Remove(categoryToDelete);
            context.SaveChanges();
            Console.WriteLine("Category and its products deleted!");
        }
    }
}