using AsyncLocal_demo.Application.Orders;
using Microsoft.EntityFrameworkCore;

namespace AsyncLocal_demo.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TenantId).IsRequired().HasMaxLength(50);
            e.Property(x => x.CreatedBy).IsRequired().HasMaxLength(100);
            e.Property(x => x.CorrelationId).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.TenantId);
            e.HasMany(x => x.Items).WithOne().HasForeignKey("OrderId").OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductId).IsRequired().HasMaxLength(50);
            e.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });
    }
}