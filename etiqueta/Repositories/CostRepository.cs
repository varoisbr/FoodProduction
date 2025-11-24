using Microsoft.EntityFrameworkCore;
using ZebraLabelPrinter.Data;
using ZebraLabelPrinter.Models;

namespace ZebraLabelPrinter.Repositories;

/// <summary>
/// Repository for managing Cost entities
/// Provides CRUD operations and common queries for cost records
/// </summary>
public class CostRepository
{
    /// <summary>
    /// Get all costs with related production data
    /// </summary>
    public List<Cost> GetAll()
    {
        using var context = new AppDbContext();
        return context.Costs
            .Include(c => c.Production)
            .OrderByDescending(c => c.Date)
            .ToList();
    }

    /// <summary>
    /// Get costs by date range
    /// </summary>
    public List<Cost> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        using var context = new AppDbContext();
        return context.Costs
            .Include(c => c.Production)
            .Where(c => c.Date >= startDate && c.Date <= endDate)
            .OrderByDescending(c => c.Date)
            .ToList();
    }

    /// <summary>
    /// Get costs by production name
    /// </summary>
    public List<Cost> GetByProductionName(string productionName)
    {
        using var context = new AppDbContext();
        return context.Costs
            .Include(c => c.Production)
            .Where(c => c.ProductionName == productionName)
            .OrderByDescending(c => c.Date)
            .ToList();
    }

    /// <summary>
    /// Get a cost by ID
    /// </summary>
    public Cost? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.Costs
            .Include(c => c.Production)
            .FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Add a new cost entry
    /// </summary>
    public Cost Add(Cost cost)
    {
        using var context = new AppDbContext();
        context.Costs.Add(cost);
        context.SaveChanges();
        return cost;
    }

    /// <summary>
    /// Update an existing cost entry
    /// </summary>
    public void Update(Cost cost)
    {
        using var context = new AppDbContext();
        context.Costs.Update(cost);
        context.SaveChanges();
    }

    /// <summary>
    /// Delete a cost by ID
    /// </summary>
    public bool Delete(int id)
    {
        using var context = new AppDbContext();
        var cost = context.Costs.Find(id);
        if (cost == null) return false;

        context.Costs.Remove(cost);
        context.SaveChanges();
        return true;
    }

    /// <summary>
    /// Get total costs for a date range
    /// </summary>
    public decimal GetTotalCosts(DateTime startDate, DateTime endDate)
    {
        using var context = new AppDbContext();
        var costs = context.Costs
            .Where(c => c.Date >= startDate && c.Date <= endDate)
            .ToList();

        return costs.Any() ? costs.Sum(c => c.TotalCost) : 0;
    }

    /// <summary>
    /// Get cost summary grouped by production
    /// </summary>
    public List<CostSummary> GetCostSummary(DateTime startDate, DateTime endDate)
    {
        using var context = new AppDbContext();
        var costs = context.Costs
            .Where(c => c.Date >= startDate && c.Date <= endDate)
            .ToList();

        return costs
            .GroupBy(c => c.ProductionName)
            .Select(g => new CostSummary
            {
                ProductionName = g.Key,
                TotalCost = g.Sum(c => c.TotalCost),
                Count = g.Count()
            })
            .ToList();
    }
}

/// <summary>
/// Summary data for cost reports
/// </summary>
public class CostSummary
{
    public string ProductionName { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public int Count { get; set; }
}
