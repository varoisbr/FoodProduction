using FoodProduction.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FoodProduction.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int ProductCount { get; set; }

    public int IngredientCount { get; set; }

    public int TemplateCount { get; set; }

    public int ActiveBatchCount { get; set; }

    public async Task OnGetAsync()
    {
        ProductCount = await _context.Products.CountAsync();
        IngredientCount = await _context.Ingredients.CountAsync();
        TemplateCount = await _context.LabelTemplates.CountAsync();
        ActiveBatchCount = await _context.ProductionBatches.CountAsync();
    }
}
