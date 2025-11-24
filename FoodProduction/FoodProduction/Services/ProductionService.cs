using FoodProduction.Data;
using FoodProduction.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodProduction.Services;

public interface IProductionService
{
    Task<ProductionBatch> CreateBatchAsync(int productId, decimal gainPercentage, string? notes = null);

    Task<Pack> AddPackAsync(int batchId, decimal weightKg);

    Task<ProductionBatch?> GetBatchAsync(int batchId);

    Task<List<Pack>> GetBatchPacksAsync(int batchId);

    Task<BatchSummary> GetBatchSummaryAsync(int batchId);

    Task<bool> MarkPackAsPrintedAsync(int packId);
}

public class ProductionService : IProductionService
{
    private readonly ApplicationDbContext _context;
    private readonly IFormulationService _formulationService;

    public ProductionService(ApplicationDbContext context, IFormulationService formulationService)
    {
        _context = context;
        _formulationService = formulationService;
    }

    public async Task<ProductionBatch> CreateBatchAsync(int productId, decimal gainPercentage, string? notes = null)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            throw new ArgumentException($"Product with ID {productId} not found.");

        // Calculate batch cost (we'll use formulation at 1kg as base, then we'll accumulate as packs are added)
        var formulation = await _formulationService.CalculateFormulationAsync(productId, 1m);

        var batch = new ProductionBatch
        {
            ProductId = productId,
            StartDate = DateTime.UtcNow,
            GainPercentage = gainPercentage,
            TotalCost = formulation.TotalEstimatedCost, // Base cost for 1kg
            Notes = notes
        };

        _context.ProductionBatches.Add(batch);
        await _context.SaveChangesAsync();

        return batch;
    }

    public async Task<Pack> AddPackAsync(int batchId, decimal weightKg)
    {
        var batch = await _context.ProductionBatches
            .Include(b => b.Product)
            .FirstOrDefaultAsync(b => b.Id == batchId);

        if (batch == null)
            throw new ArgumentException($"Batch with ID {batchId} not found.");

        // Get formulation cost for the actual weight
        var formulation = await _formulationService.CalculateFormulationAsync(batch.ProductId, weightKg);
        decimal baseCost = formulation.TotalEstimatedCost;

        // Apply gain/loss percentage
        decimal finalPrice = baseCost * (1 + batch.GainPercentage / 100m);

        var pack = new Pack
        {
            BatchId = batchId,
            WeightKg = weightKg,
            Price = finalPrice,
            Printed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Packs.Add(pack);

        // Update batch total cost
        batch.TotalCost += baseCost;
        _context.ProductionBatches.Update(batch);

        await _context.SaveChangesAsync();

        return pack;
    }

    public async Task<ProductionBatch?> GetBatchAsync(int batchId)
    {
        return await _context.ProductionBatches
            .Include(b => b.Product)
            .Include(b => b.Packs)
            .FirstOrDefaultAsync(b => b.Id == batchId);
    }

    public async Task<List<Pack>> GetBatchPacksAsync(int batchId)
    {
        return await _context.Packs
            .Where(p => p.BatchId == batchId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<BatchSummary> GetBatchSummaryAsync(int batchId)
    {
        var batch = await GetBatchAsync(batchId);
        if (batch == null)
            throw new ArgumentException($"Batch with ID {batchId} not found.");

        var packs = await GetBatchPacksAsync(batchId);

        decimal totalWeight = packs.Sum(p => p.WeightKg);
        decimal totalValue = packs.Sum(p => p.Price);
        decimal averageYield = packs.Any() ? totalWeight / packs.Count : 0;

        return new BatchSummary
        {
            BatchId = batch.Id,
            ProductId = batch.ProductId,
            ProductName = batch.Product?.Name ?? "Unknown",
            StartDate = batch.StartDate,
            GainPercentage = batch.GainPercentage,
            TotalPacks = packs.Count,
            TotalWeight = totalWeight,
            AverageYield = averageYield,
            TotalValue = totalValue,
            PrintedPacks = packs.Count(p => p.Printed),
            Notes = batch.Notes
        };
    }

    public async Task<bool> MarkPackAsPrintedAsync(int packId)
    {
        var pack = await _context.Packs.FindAsync(packId);
        if (pack == null)
            return false;

        pack.Printed = true;
        _context.Packs.Update(pack);
        await _context.SaveChangesAsync();

        return true;
    }
}

public class BatchSummary
{
    public int BatchId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public decimal GainPercentage { get; set; }

    public int TotalPacks { get; set; }

    public decimal TotalWeight { get; set; }

    public decimal AverageYield { get; set; }

    public decimal TotalValue { get; set; }

    public int PrintedPacks { get; set; }

    public string? Notes { get; set; }
}
