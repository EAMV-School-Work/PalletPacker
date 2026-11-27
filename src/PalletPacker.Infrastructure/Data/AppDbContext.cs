using Microsoft.EntityFrameworkCore;
using PalletPacker.Core.Entities;

namespace PalletPacker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<PalletDefinition> PalletDefinitions => Set<PalletDefinition>();
    public DbSet<Element> Elements => Set<Element>();
    public DbSet<Order> Orders => Set<Order>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PalletDefinition>(entity =>
        {
            entity.Property(p => p.OwnWeightKg).HasPrecision(10, 2);
            entity.Property(p => p.MaxWeightKg).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Element>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.WeightKg).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Id).ValueGeneratedNever();

            entity.HasMany(o => o.Elements)
                .WithOne()
                .HasForeignKey("OrderId");
        });
    }
}