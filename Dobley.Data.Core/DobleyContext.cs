using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core;

public class DobleyContext : DbContext
{
    public DobleyContext()
    {
    }

    public DobleyContext(DbContextOptions<DobleyContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "admin";
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "admin";
            var dbName = Environment.GetEnvironmentVariable("DB_NAME")  ?? "postgres";

            var connectionString =
                $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка сущности Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products"); // Указываем имя таблицы

            entity.HasKey(p => p.Id); // Первичный ключ

            entity.Property(p => p.Name)
                .IsRequired() // Поле обязательно для заполнения
                .HasMaxLength(255); // Максимальная длина строки

            entity.Property(p => p.Description)
                .HasMaxLength(1000); // Максимальная длина строки

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18, 2)") // Тип данных для цены
                .IsRequired();

            entity.Property(p => p.Category)
                .HasMaxLength(100); // Максимальная длина строки

            entity.Property(p => p.Unit)
                .HasColumnType("decimal(18, 2)") // Тип данных для единиц
                .IsRequired();

            entity.Property(p => p.UnitType)
                .HasMaxLength(50); // Максимальная длина строки

            entity.Property(p => p.Barcode)
                .HasMaxLength(50); // Максимальная длина строки
        });

        // Настройка сущности User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users"); // Указываем имя таблицы

            entity.HasKey(u => u.Login); // Первичный ключ (Login)

            entity.Property(u => u.Password)
                .IsRequired() // Поле обязательно для заполнения
                .HasMaxLength(255); // Максимальная длина строки
        });
    }
}