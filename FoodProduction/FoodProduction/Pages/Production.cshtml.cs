using FoodProduction.Data;
using FoodProduction.Models;
using FoodProduction.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FoodProduction.Pages;

public class ProductionModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IProductionService _productionService;
    private readonly IZplPrinterService _zplPrinterService;
    private readonly ILogger<ProductionModel> _logger;

    public ProductionModel(ApplicationDbContext context, IProductionService productionService, IZplPrinterService zplPrinterService, ILogger<ProductionModel> logger)
    {
        _context = context;
        _productionService = productionService;
        _zplPrinterService = zplPrinterService;
        _logger = logger;
    }

    public List<Product> Products { get; set; } = [];

    public ProductionBatch? CurrentBatch { get; set; }

    public List<Pack> Packs { get; set; } = [];

    public BatchSummary? BatchSummary { get; set; }

    public string? SuccessMessage { get; set; }

    [BindProperty]
    public int ProductId { get; set; }

    [BindProperty]
    public decimal GainPercentage { get; set; }

    [BindProperty]
    public string? Notes { get; set; }

    [BindProperty]
    public decimal WeightKg { get; set; }

    public async Task OnGetAsync()
    {
        Products = await _context.Products.OrderBy(p => p.Name).ToListAsync();

        // Load the current batch from session or database
        if (int.TryParse(HttpContext.Session.GetString("CurrentBatchId"), out int batchId))
        {
            CurrentBatch = await _productionService.GetBatchAsync(batchId);
            if (CurrentBatch != null)
            {
                Packs = await _productionService.GetBatchPacksAsync(batchId);
                BatchSummary = await _productionService.GetBatchSummaryAsync(batchId);
            }
        }
    }

    public async Task<IActionResult> OnPostAsync(string action)
    {
        Products = await _context.Products.OrderBy(p => p.Name).ToListAsync();

        try
        {
            if (action == "createBatch")
            {
                var batch = await _productionService.CreateBatchAsync(ProductId, GainPercentage, Notes);
                HttpContext.Session.SetString("CurrentBatchId", batch.Id.ToString());

                CurrentBatch = batch;
                SuccessMessage = $"Production batch created for {batch.Product?.Name} (ID: {batch.Id})";
            }
            else if (action == "addPack")
            {
                var batchIdStr = HttpContext.Session.GetString("CurrentBatchId");
                if (!int.TryParse(batchIdStr, out int batchId))
                {
                    ModelState.AddModelError(string.Empty, "No active batch found.");
                    return Page();
                }

                var pack = await _productionService.AddPackAsync(batchId, WeightKg);
                CurrentBatch = await _productionService.GetBatchAsync(batchId);

                // Generate and print ZPL label
                if (CurrentBatch?.Product?.DefaultLabelTemplateId.HasValue ?? false)
                {
                    var template = await _zplPrinterService.GetTemplateAsync(CurrentBatch.Product.DefaultLabelTemplateId.Value);
                    if (template != null)
                    {
                        var zplContent = await _zplPrinterService.RenderLabelAsync(
                            template,
                            CurrentBatch.Product.Name,
                            pack.WeightKg,
                            pack.Price,
                            DateTime.UtcNow
                        );

                        // Try to print to physical printer
                        var printSuccess = await _zplPrinterService.PrintLabelAsync(zplContent);

                        if (!printSuccess)
                        {
                            // Save to file as fallback
                            var fileName = Path.Combine(
                                "labels",
                                $"Pack_{pack.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.zpl"
                            );

                            Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);
                            await _zplPrinterService.PrintLabelToFileAsync(zplContent, fileName);

                            _logger.LogWarning("Could not connect to physical printer. Label saved to file: {FileName}", fileName);
                        }

                        // Mark pack as printed
                        await _productionService.MarkPackAsPrintedAsync(pack.Id);
                    }
                }

                Packs = await _productionService.GetBatchPacksAsync(batchId);
                BatchSummary = await _productionService.GetBatchSummaryAsync(batchId);

                SuccessMessage = $"Pack added: {WeightKg} kg @ {pack.Price:C2}. Label printed successfully.";
            }
            else if (action == "endBatch")
            {
                HttpContext.Session.Remove("CurrentBatchId");
                CurrentBatch = null;
                SuccessMessage = "Production batch ended.";
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            _logger.LogError(ex, "Error in production operation: {Action}", action);
        }

        return Page();
    }
}
