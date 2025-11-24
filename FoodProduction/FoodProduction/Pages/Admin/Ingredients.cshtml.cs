using FoodProduction.Data;
using FoodProduction.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FoodProduction.Pages.Admin;

public class IngredientsAdminModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IngredientsAdminModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Ingredient> Ingredients { get; set; } = [];

    [BindProperty]
    public int EditingIngredientId { get; set; }

    [BindProperty]
    public string? IngredientName { get; set; }

    [BindProperty]
    public decimal CostPerKg { get; set; }

    public async Task OnGetAsync()
    {
        await LoadData();
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        await LoadData();

        try
        {
            if (action == "save")
            {
                if (string.IsNullOrWhiteSpace(IngredientName))
                {
                    ModelState.AddModelError(string.Empty, "Ingredient name is required.");
                    return Page();
                }

                if (EditingIngredientId > 0)
                {
                    // Update existing ingredient
                    var ingredient = await _context.Ingredients.FindAsync(EditingIngredientId);
                    if (ingredient == null)
                    {
                        ModelState.AddModelError(string.Empty, "Ingredient not found.");
                        return Page();
                    }

                    ingredient.Name = IngredientName;
                    ingredient.CostPerKg = CostPerKg;

                    _context.Ingredients.Update(ingredient);
                }
                else
                {
                    // Create new ingredient
                    var ingredient = new Ingredient
                    {
                        Name = IngredientName,
                        CostPerKg = CostPerKg
                    };

                    _context.Ingredients.Add(ingredient);
                }

                await _context.SaveChangesAsync();
            }
            else if (action == "cancel")
            {
                EditingIngredientId = 0;
                IngredientName = null;
                CostPerKg = 0;
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostEditAsync(int id)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);
        if (ingredient == null)
        {
            return NotFound();
        }

        EditingIngredientId = ingredient.Id;
        IngredientName = ingredient.Name;
        CostPerKg = ingredient.CostPerKg;

        await LoadData();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);
        if (ingredient != null)
        {
            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private async Task LoadData()
    {
        Ingredients = await _context.Ingredients
            .Include(i => i.ProductFormulations)
            .OrderBy(i => i.Name)
            .ToListAsync();
    }
}
