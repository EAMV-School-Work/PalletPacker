using Microsoft.EntityFrameworkCore;
using PalletPacker.Core.Entities;

namespace PalletPacker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<PalletDefinition> PalletDefinitions => Set<PalletDefinition>();

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
    }
}