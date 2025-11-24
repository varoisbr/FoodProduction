using FoodProduction.Data;
using FoodProduction.Models;
using FoodProduction.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FoodProduction.Pages;

public class FormulationModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IFormulationService _formulationService;
    private readonly IExportService _exportService;

    public FormulationModel(ApplicationDbContext context, IFormulationService formulationService, IExportService exportService)
    {
        _context = context;
        _formulationService = formulationService;
        _exportService = exportService;
    }

    public List<Product> Products { get; set; } = [];

    [BindProperty]
    public int SelectedProductId { get; set; }

    [BindProperty]
    public decimal TargetWeight { get; set; } = 1m;

    public FormulationResult? FormulationResult { get; set; }

    public async Task OnGetAsync()
    {
        Products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task OnPostAsync()
    {
        Products = await _context.Products.OrderBy(p => p.Name).ToListAsync();

        if (SelectedProductId > 0 && TargetWeight > 0)
        {
            try
            {
                FormulationResult = await _formulationService.CalculateFormulationAsync(SelectedProductId, TargetWeight);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            }
        }
    }

    public async Task<IActionResult> OnPostExportPdfAsync(int productId, decimal targetWeight)
    {
        try
        {
            var formulation = await _formulationService.CalculateFormulationAsync(productId, targetWeight);
            var pdfBytes = await _exportService.ExportFormulationToPdfAsync(formulation);

            var fileName = $"{formulation.ProductName.Replace(" ", "_")}_Formulation_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error exporting PDF: {ex.Message}");
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnPostExportCsvAsync(int productId, decimal targetWeight)
    {
        try
        {
            var formulation = await _formulationService.CalculateFormulationAsync(productId, targetWeight);
            var csvBytes = await _exportService.ExportFormulationToCsvAsync(formulation);

            var fileName = $"{formulation.ProductName.Replace(" ", "_")}_Formulation_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(csvBytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error exporting CSV: {ex.Message}");
            return RedirectToPage();
        }
    }
}
