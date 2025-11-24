using FoodProduction.Data;
using FoodProduction.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodProduction.Services;

public interface IFormulationService
{
    Task<Product?> GetProductAsync(int productId);

    Task<List<ProductFormulation>> GetProductFormulationsAsync(int productId);

    Task<FormulationResult> CalculateFormulationAsync(int productId, decimal targetWeightKg);
}

public class FormulationService : IFormulationService
{
    private readonly ApplicationDbContext _context;

    public FormulationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetProductAsync(int productId)
    {
        return await _context.Products
            .Include(p => p.Formulations)
                .ThenInclude(pf => pf.Ingredient)
            .FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task<List<ProductFormulation>> GetProductFormulationsAsync(int productId)
    {
        return await _context.ProductFormulations
            .Where(pf => pf.ProductId == productId)
            .Include(pf => pf.Ingredient)
            .OrderBy(pf => pf.Ingredient!.Name)
            .ToListAsync();
    }

    public async Task<FormulationResult> CalculateFormulationAsync(int productId, decimal targetWeightKg)
    {
        var product = await GetProductAsync(productId);
        if (product == null)
            throw new ArgumentException($"Product with ID {productId} not found.");

        var formulations = await GetProductFormulationsAsync(productId);
        if (!formulations.Any())
            throw new InvalidOperationException($"No formulations found for product {product.Name}.");

        var result = new FormulationResult
        {
            ProductId = productId,
            ProductName = product.Name,
            TargetWeightKg = targetWeightKg,
            Ingredients = []
        };

        decimal totalCost = 0;

        foreach (var formulation in formulations)
        {
            var ingredient = formulation.Ingredient;
            if (ingredient == null)
                continue;

            decimal calculatedWeight = (targetWeightKg * formulation.Ratio) / 100m;
            decimal subtotalCost = calculatedWeight * ingredient.CostPerKg;

            result.Ingredients.Add(new IngredientFormulation
            {
                IngredientId = ingredient.Id,
                IngredientName = ingredient.Name,
                Ratio = formulation.Ratio,
                CalculatedWeightKg = calculatedWeight,
                CostPerKg = ingredient.CostPerKg,
                SubtotalCost = subtotalCost
            });

            totalCost += subtotalCost;
        }

        result.TotalEstimatedCost = totalCost;
        return result;
    }
}

public class FormulationResult
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal TargetWeightKg { get; set; }

    public List<IngredientFormulation> Ingredients { get; set; } = [];

    public decimal TotalEstimatedCost { get; set; }
}

public class IngredientFormulation
{
    public int IngredientId { get; set; }

    public string IngredientName { get; set; } = string.Empty;

    public decimal Ratio { get; set; } // Percentage

    public decimal CalculatedWeightKg { get; set; }

    public decimal CostPerKg { get; set; }

    public decimal SubtotalCost { get; set; }
}
