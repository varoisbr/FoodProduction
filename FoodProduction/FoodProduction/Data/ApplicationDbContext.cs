using FoodProduction.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodProduction.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    public DbSet<ProductFormulation> ProductFormulations => Set<ProductFormulation>();

    public DbSet<ProductionBatch> ProductionBatches => Set<ProductionBatch>();

    public DbSet<Pack> Packs => Set<Pack>();

    public DbSet<LabelTemplate> LabelTemplates => Set<LabelTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.Property(e => e.DefaultGainPercentage).HasPrecision(5, 2);
            entity.HasMany(e => e.Formulations)
                .WithOne(pf => pf.Product)
                .HasForeignKey(pf => pf.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.ProductionBatches)
                .WithOne(pb => pb.Product)
                .HasForeignKey(pb => pb.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Ingredient configuration
        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.Property(e => e.CostPerKg).HasPrecision(10, 4);
        });

        // ProductFormulation configuration
        modelBuilder.Entity<ProductFormulation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ratio).HasPrecision(5, 2);
            entity.HasOne(pf => pf.Product)
                .WithMany(p => p.Formulations)
                .HasForeignKey(pf => pf.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pf => pf.Ingredient)
                .WithMany(i => i.ProductFormulations)
                .HasForeignKey(pf => pf.IngredientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductionBatch configuration
        modelBuilder.Entity<ProductionBatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartDate).HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.Property(e => e.GainPercentage).HasPrecision(5, 2);
            entity.Property(e => e.TotalCost).HasPrecision(10, 4);
            entity.HasMany(e => e.Packs)
                .WithOne(p => p.Batch)
                .HasForeignKey(p => p.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Pack configuration
        modelBuilder.Entity<Pack>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeightKg).HasPrecision(10, 4);
            entity.Property(e => e.Price).HasPrecision(10, 4);
            entity.Property(e => e.CreatedAt).HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        });

        // LabelTemplate configuration
        modelBuilder.Entity<LabelTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.CreatedAt).HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            entity.Property(e => e.UpdatedAt).HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        });
    }
}
