using FoodProduction.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodProduction.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductionController : ControllerBase
{
    private readonly IProductionService _productionService;
    private readonly IZplPrinterService _zplPrinterService;

    public ProductionController(IProductionService productionService, IZplPrinterService zplPrinterService)
    {
        _productionService = productionService;
        _zplPrinterService = zplPrinterService;
    }

    [HttpPost("batch/create")]
    public async Task<ActionResult<dynamic>> CreateBatch([FromBody] CreateBatchRequest request)
    {
        try
        {
            var batch = await _productionService.CreateBatchAsync(request.ProductId, request.GainPercentage, request.Notes);
            return Ok(new { batchId = batch.Id, message = "Batch created successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("pack/add")]
    public async Task<ActionResult<dynamic>> AddPack([FromBody] AddPackRequest request)
    {
        try
        {
            var pack = await _productionService.AddPackAsync(request.BatchId, request.WeightKg);
            return Ok(new { packId = pack.Id, price = pack.Price, message = "Pack added successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("batch/{batchId}/summary")]
    public async Task<ActionResult<BatchSummary>> GetBatchSummary(int batchId)
    {
        try
        {
            var summary = await _productionService.GetBatchSummaryAsync(batchId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}

public class CreateBatchRequest
{
    public int ProductId { get; set; }

    public decimal GainPercentage { get; set; }

    public string? Notes { get; set; }
}

public class AddPackRequest
{
    public int BatchId { get; set; }

    public decimal WeightKg { get; set; }
}
