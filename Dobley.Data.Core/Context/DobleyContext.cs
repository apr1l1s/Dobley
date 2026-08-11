using Dobley.Domain.Core.Entities.Products;
using Dobley.Domain.Core.Entities.Notifications;
using Dobley.Domain.Core.Entities.Storages;
using Dobley.Domain.Core.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Dobley.Data.Core.Context;

public class DobleyContext
    : DbContext
{
    public DobleyContext()
    {
    }

    public DobleyContext(DbContextOptions<DobleyContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    public DbSet<NotificationInvite> NotificationInvites { get; set; }

    public DbSet<NotificationRecipient> NotificationRecipients { get; set; }

    public DbSet<Storage> Storages { get; set; }

    public DbSet<StorageNotificationSubscription> StorageNotificationSubscriptions { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "admin";
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "admin";
            var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "postgres";

            var connectionString =
                $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasQueryFilter(p => p.DateDeleted == null);

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Description)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            entity.Property(p => p.Category)
                .HasConversion<string>()
                .HasMaxLength(100);

            entity.Property(p => p.Unit)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            entity.Property(p => p.UnitType)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(p => p.Barcode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.ExpirationDate);

            entity.HasOne(p => p.DomainStorage)
                .WithMany()
                .HasForeignKey(p => p.StorageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationInvite>(entity =>
        {
            entity.ToTable("NotificationInvites");
            entity.HasQueryFilter(x => x.DateDeleted == null);

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.HasOne(x => x.DomainUser)
                .WithMany()
                .HasForeignKey(x => x.UserName)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.ToTable("NotificationRecipients");
            entity.HasQueryFilter(x => x.DateDeleted == null);

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Channel)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(x => x.ExternalId)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.DisplayName)
                .HasMaxLength(200);

            entity.HasIndex(x => new { x.UserName, x.Channel, x.ExternalId })
                .HasFilter("\"DateDeleted\" IS NULL")
                .IsUnique();

            entity.HasOne(x => x.DomainUser)
                .WithMany()
                .HasForeignKey(x => x.UserName)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Storage>(entity =>
        {
            entity.ToTable("Storages");
            entity.HasQueryFilter(s => s.DateDeleted == null);

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(s => s.Description)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.UserName)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(s => s.DomainUser)
                .WithMany()
                .HasForeignKey(s => s.UserName)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StorageNotificationSubscription>(entity =>
        {
            entity.ToTable("StorageNotificationSubscriptions");
            entity.HasQueryFilter(x => x.DateDeleted == null);

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.NotificationRecipientId, x.StorageId })
                .HasFilter("\"DateDeleted\" IS NULL")
                .IsUnique();

            entity.HasOne(x => x.DomainNotificationRecipient)
                .WithMany()
                .HasForeignKey(x => x.NotificationRecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.DomainStorage)
                .WithMany()
                .HasForeignKey(x => x.StorageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasQueryFilter(u => u.DateDeleted == null);

            entity.HasKey(u => u.Login);

            entity.Property(u => u.Login)
                .HasMaxLength(100);

            entity.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(255);
        });
    }
}
