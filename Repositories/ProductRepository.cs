using Microsoft.EntityFrameworkCore;
using ZebraLabelPrinter.Data;
using ZebraLabelPrinter.Models;

namespace ZebraLabelPrinter.Repositories;

/// <summary>
/// Repository for managing Product entities
/// Provides CRUD operations and common queries for products
/// </summary>
public class ProductRepository
{
    /// <summary>
    /// Get all products from the database
    /// </summary>
    public List<Product> GetAll()
    {
        using var context = new AppDbContext();
        return context.Products.OrderBy(p => p.Name).ToList();
    }

    /// <summary>
    /// Get all products of a specific type (Finished or Raw)
    /// </summary>
    public List<Product> GetByType(string type)
    {
        using var context = new AppDbContext();
        return context.Products
            .Where(p => p.Type == type)
            .OrderBy(p => p.Name)
            .ToList();
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    public Product? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.Products.Find(id);
    }

    /// <summary>
    /// Add a new product
    /// </summary>
    public Product Add(Product product)
    {
        using var context = new AppDbContext();
        context.Products.Add(product);
        context.SaveChanges();
        return product;
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    public void Update(Product product)
    {
        using var context = new AppDbContext();
        context.Products.Update(product);
        context.SaveChanges();
    }

    /// <summary>
    /// Delete a product by ID
    /// Note: This may fail if product is referenced by productions or formulations
    /// </summary>
    public bool Delete(int id)
    {
        using var context = new AppDbContext();
        var product = context.Products.Find(id);
        if (product == null) return false;

        context.Products.Remove(product);
        context.SaveChanges();
        return true;
    }

    /// <summary>
    /// Check if a product name already exists (for validation)
    /// </summary>
    public bool NameExists(string name, int? excludeId = null)
    {
        using var context = new AppDbContext();
        var query = context.Products.Where(p => p.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return query.Any();
    }
}
