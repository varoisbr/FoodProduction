using FoodProduction.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodProduction.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrinterController : ControllerBase
{
    private readonly IZplPrinterService _zplPrinterService;

    public PrinterController(IZplPrinterService zplPrinterService)
    {
        _zplPrinterService = zplPrinterService;
    }

    [HttpPost("print")]
    public async Task<ActionResult<dynamic>> PrintLabel([FromBody] PrintLabelRequest request)
    {
        try
        {
            var success = await _zplPrinterService.PrintLabelAsync(request.ZplContent);
            if (success)
                return Ok(new { message = "Label sent to printer successfully" });
            else
                return BadRequest(new { error = "Failed to connect to printer" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("render")]
    public async Task<ActionResult<dynamic>> RenderLabel([FromBody] RenderLabelRequest request)
    {
        try
        {
            var template = await _zplPrinterService.GetTemplateAsync(request.TemplateId);
            if (template == null)
                return NotFound(new { error = "Template not found" });

            var zplContent = await _zplPrinterService.RenderLabelAsync(
                template,
                request.ProductName,
                request.Weight,
                request.Price,
                DateTime.UtcNow
            );

            return Ok(new { zplContent });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("templates")]
    public async Task<ActionResult<dynamic>> GetTemplates()
    {
        try
        {
            var templates = await _zplPrinterService.GetAllTemplatesAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class PrintLabelRequest
{
    public required string ZplContent { get; set; }
}

public class RenderLabelRequest
{
    public int TemplateId { get; set; }

    public required string ProductName { get; set; }

    public decimal Weight { get; set; }

    public decimal Price { get; set; }
}
