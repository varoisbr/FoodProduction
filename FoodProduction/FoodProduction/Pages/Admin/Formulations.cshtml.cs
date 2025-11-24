using FoodProduction.Data;
using FoodProduction.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FoodProduction.Pages.Admin;

public class FormulationsAdminModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public FormulationsAdminModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Product> Products { get; set; } = [];

    public List<Ingredient> Ingredients { get; set; } = [];

    public List<ProductFormulation> ProductFormulations { get; set; } = [];

    [BindProperty]
    public int SelectedProductId { get; set; }

    [BindProperty]
    public int IngredientId { get; set; }

    [BindProperty]
    public decimal Ratio { get; set; }

    public async Task OnGetAsync()
    {
        await LoadData();
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        await LoadData();

        try
        {
            if (action == "addIngredient")
            {
                if (SelectedProductId <= 0)
                {
                    ModelState.AddModelError(string.Empty, "Please select a product.");
                    return Page();
                }

                if (IngredientId <= 0)
                {
                    ModelState.AddModelError(string.Empty, "Please select an ingredient.");
                    return Page();
                }

                if (Ratio <= 0 || Ratio > 100)
                {
                    ModelState.AddModelError(string.Empty, "Ratio must be between 0 and 100.");
                    return Page();
                }

                // Check if this ingredient is already in the product
                var existing = await _context.ProductFormulations
                    .FirstOrDefaultAsync(pf => pf.ProductId == SelectedProductId && pf.IngredientId == IngredientId);

                if (existing != null)
                {
                    existing.Ratio = Ratio;
                    _context.ProductFormulations.Update(existing);
                }
                else
                {
                    var formulation = new ProductFormulation
                    {
                        ProductId = SelectedProductId,
                        IngredientId = IngredientId,
                        Ratio = Ratio
                    };

                    _context.ProductFormulations.Add(formulation);
                }

                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostRemoveIngredientAsync(int id)
    {
        var formulation = await _context.ProductFormulations.FindAsync(id);
        if (formulation != null)
        {
            SelectedProductId = formulation.ProductId;
            _context.ProductFormulations.Remove(formulation);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { productId = SelectedProductId });
    }

    private async Task LoadData()
    {
        Products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
        Ingredients = await _context.Ingredients.OrderBy(i => i.Name).ToListAsync();

        if (SelectedProductId > 0)
        {
            ProductFormulations = await _context.ProductFormulations
                .Where(pf => pf.ProductId == SelectedProductId)
                .Include(pf => pf.Ingredient)
                .OrderBy(pf => pf.Ingredient!.Name)
                .ToListAsync();
        }
    }
}
