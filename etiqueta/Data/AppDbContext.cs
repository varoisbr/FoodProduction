using Microsoft.EntityFrameworkCore;
using ZebraLabelPrinter.Models;

namespace ZebraLabelPrinter.Data;

/// <summary>
/// Entity Framework Core database context for the application
/// Manages all database operations and entity relationships
/// Uses SQLite as the database provider (local file: data.db)
///
/// FUTURE API INTEGRATION NOTES:
/// - This DbContext can be used directly by an ASP.NET Core Web API
/// - For Flutter app sync, create API endpoints that expose:
///   1. POST /api/sales - to receive sales data from Flutter
///   2. GET /api/productions - to send production data to Flutter
///   3. GET /api/products - to sync product catalog
/// - Consider adding a Sales table in the future to track actual sales
/// - Add sync timestamps to track last sync with mobile app
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Products table - stores both finished products and raw materials
    /// </summary>
    public DbSet<Product> Products { get; set; } = null!;

    /// <summary>
    /// Formulations table - stores recipes for products
    /// </summary>
    public DbSet<Formulation> Formulations { get; set; } = null!;

    /// <summary>
    /// FormulationIngredients table - stores ingredients for each formulation
    /// </summary>
    public DbSet<FormulationIngredient> FormulationIngredients { get; set; } = null!;

    /// <summary>
    /// Productions table - stores all production entries (replaces CSV files)
    /// </summary>
    public DbSet<Production> Productions { get; set; } = null!;

    /// <summary>
    /// Costs table - stores production costs for profit calculation
    /// </summary>
    public DbSet<Cost> Costs { get; set; } = null!;

    /// <summary>
    /// Database file path - stored in the application directory
    /// </summary>
    private static readonly string DbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "data.db"
    );

    /// <summary>
    /// Configure the database connection to use SQLite
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }
    }

    /// <summary>
    /// Configure entity relationships and constraints
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.Type).HasDefaultValue("Finished");
        });

        // Formulation configuration
        modelBuilder.Entity<Formulation>(entity =>
        {
            entity.HasIndex(e => e.Name);
        });

        // FormulationIngredient configuration
        modelBuilder.Entity<FormulationIngredient>(entity =>
        {
            // Ensure cascading delete when formulation is deleted
            entity.HasOne(e => e.Formulation)
                .WithMany(f => f.Ingredients)
                .HasForeignKey(e => e.FormulationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Don't cascade delete when product is deleted (preserve historical data)
            entity.HasOne(e => e.Product)
                .WithMany(p => p.FormulationIngredients)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Production configuration
        modelBuilder.Entity<Production>(entity =>
        {
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.Name);

            // Don't cascade delete when product is deleted (preserve historical data)
            entity.HasOne(e => e.Product)
                .WithMany(p => p.Productions)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Cost configuration
        modelBuilder.Entity<Cost>(entity =>
        {
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.ProductionName);

            // Don't cascade delete when production is deleted (preserve cost records)
            entity.HasOne(e => e.Production)
                .WithMany(p => p.Costs)
                .HasForeignKey(e => e.ProductionId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    /// <summary>
    /// Ensure database and tables are created
    /// Call this at application startup
    /// </summary>
    public static void EnsureCreated()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();

        // Optional: Add seed data for testing
        SeedData(context);
    }

    /// <summary>
    /// Seed initial data for testing (optional)
    /// </summary>
    private static void SeedData(AppDbContext context)
    {
        // Only seed if database is empty
        if (!context.Products.Any())
        {
            // Add some sample raw materials
            context.Products.AddRange(
                new Product { Name = "Sal", Type = "Raw", DefaultPricePerKg = 2.50m },
                new Product { Name = "Pimenta do Reino", Type = "Raw", DefaultPricePerKg = 15.00m },
                new Product { Name = "Alho em Po", Type = "Raw", DefaultPricePerKg = 12.00m }
            );

            context.SaveChanges();
        }
    }

    /// <summary>
    /// Get the full path to the database file
    /// Useful for debugging or displaying to the user
    /// </summary>
    public static string GetDatabasePath() => DbPath;
}
