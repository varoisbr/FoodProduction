using FoodProduction.Data;
using FoodProduction.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FoodProduction.Pages.Admin;

public class ProductsAdminModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public ProductsAdminModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Product> Products { get; set; } = [];

    public List<LabelTemplate> LabelTemplates { get; set; } = [];

    [BindProperty]
    public int EditingProductId { get; set; }

    [BindProperty]
    public string? ProductName { get; set; }

    [BindProperty]
    public decimal DefaultGainPercentage { get; set; }

    [BindProperty]
    public int? LabelTemplateId { get; set; }

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
                if (string.IsNullOrWhiteSpace(ProductName))
                {
                    ModelState.AddModelError(string.Empty, "Product name is required.");
                    return Page();
                }

                if (EditingProductId > 0)
                {
                    // Update existing product
                    var product = await _context.Products.FindAsync(EditingProductId);
                    if (product == null)
                    {
                        ModelState.AddModelError(string.Empty, "Product not found.");
                        return Page();
                    }

                    product.Name = ProductName;
                    product.DefaultGainPercentage = DefaultGainPercentage;
                    product.DefaultLabelTemplateId = LabelTemplateId;

                    _context.Products.Update(product);
                }
                else
                {
                    // Create new product
                    var product = new Product
                    {
                        Name = ProductName,
                        DefaultGainPercentage = DefaultGainPercentage,
                        DefaultLabelTemplateId = LabelTemplateId
                    };

                    _context.Products.Add(product);
                }

                await _context.SaveChangesAsync();
            }
            else if (action == "cancel")
            {
                EditingProductId = 0;
                ProductName = null;
                DefaultGainPercentage = 0;
                LabelTemplateId = null;
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
        var product = await _context.Products
            .Include(p => p.DefaultLabelTemplate)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        EditingProductId = product.Id;
        ProductName = product.Name;
        DefaultGainPercentage = product.DefaultGainPercentage;
        LabelTemplateId = product.DefaultLabelTemplateId;

        await LoadData();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private async Task LoadData()
    {
        Products = await _context.Products
            .Include(p => p.DefaultLabelTemplate)
            .OrderBy(p => p.Name)
            .ToListAsync();

        LabelTemplates = await _context.LabelTemplates
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}
