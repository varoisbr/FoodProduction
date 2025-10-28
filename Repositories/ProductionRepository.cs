using Microsoft.EntityFrameworkCore;
using ZebraLabelPrinter.Data;
using ZebraLabelPrinter.Models;

namespace ZebraLabelPrinter.Repositories;

/// <summary>
/// Repository for managing Production entities
/// Provides CRUD operations and common queries for production records
/// This replaces the old CSV file storage system
/// </summary>
public class ProductionRepository
{
    /// <summary>
    /// Get all productions with related product data
    /// </summary>
    public List<Production> GetAll()
    {
        using var context = new AppDbContext();
        return context.Productions
            .Include(p => p.Product)
            .OrderByDescending(p => p.Date)
            .ToList();
    }

    /// <summary>
    /// Get productions by date range
    /// </summary>
    public List<Production> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        using var context = new AppDbContext();
        return context.Productions
            .Include(p => p.Product)
            .Where(p => p.Date >= startDate && p.Date <= endDate)
            .OrderByDescending(p => p.Date)
            .ToList();
    }

    /// <summary>
    /// Get productions by batch name
    /// </summary>
    public List<Production> GetByName(string name)
    {
        using var context = new AppDbContext();
        return context.Productions
            .Include(p => p.Product)
            .Where(p => p.Name == name)
            .OrderByDescending(p => p.Date)
            .ToList();
    }

    /// <summary>
    /// Get a production by ID
    /// </summary>
    public Production? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.Productions
            .Include(p => p.Product)
            .FirstOrDefault(p => p.Id == id);
    }

    /// <summary>
    /// Add a new production entry
    /// </summary>
    public Production Add(Production production)
    {
        using var context = new AppDbContext();
        context.Productions.Add(production);
        context.SaveChanges();
        return production;
    }

    /// <summary>
    /// Update an existing production entry
    /// </summary>
    public void Update(Production production)
    {
        using var context = new AppDbContext();
        context.Productions.Update(production);
        context.SaveChanges();
    }

    /// <summary>
    /// Delete a production by ID
    /// </summary>
    public bool Delete(int id)
    {
        using var context = new AppDbContext();
        var production = context.Productions.Find(id);
        if (production == null) return false;

        context.Productions.Remove(production);
        context.SaveChanges();
        return true;
    }

    /// <summary>
    /// Get total production value for a date range
    /// </summary>
    public decimal GetTotalValue(DateTime startDate, DateTime endDate)
    {
        using var context = new AppDbContext();
        var productions = context.Productions
            .Where(p => p.Date >= startDate && p.Date <= endDate)
            .ToList();

        return productions.Any() ? productions.Sum(p => p.Total) : 0;
    }

    /// <summary>
    /// Get total weight produced for a date range
    /// </summary>
    public decimal GetTotalWeight(DateTime startDate, DateTime endDate)
    {
        using var context = new AppDbContext();
        var productions = context.Productions
            .Where(p => p.Date >= startDate && p.Date <= endDate)
            .ToList();

        return productions.Any() ? productions.Sum(p => p.Weight) : 0;
    }

    /// <summary>
    /// Get production summary grouped by product
    /// </summary>
    public List<ProductionSummary> GetProductionSummary(DateTime startDate, DateTime endDate)
    {
        using var context = new AppDbContext();
        var productions = context.Productions
            .Include(p => p.Product)
            .Where(p => p.Date >= startDate && p.Date <= endDate)
            .ToList();

        return productions
            .GroupBy(p => p.Product != null ? p.Product.Name : "Sem Produto")
            .Select(g => new ProductionSummary
            {
                ProductName = g.Key,
                TotalWeight = g.Sum(p => p.Weight),
                TotalValue = g.Sum(p => p.Total),
                Count = g.Count()
            })
            .ToList();
    }
}

/// <summary>
/// Summary data for production reports
/// </summary>
public class ProductionSummary
{
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalWeight { get; set; }
    public decimal TotalValue { get; set; }
    public int Count { get; set; }
}
