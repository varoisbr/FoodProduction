using FoodProduction.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodProduction.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormulationController : ControllerBase
{
    private readonly IFormulationService _formulationService;

    public FormulationController(IFormulationService formulationService)
    {
        _formulationService = formulationService;
    }

    [HttpGet("calculate/{productId}/{targetWeightKg}")]
    public async Task<ActionResult<FormulationResult>> Calculate(int productId, decimal targetWeightKg)
    {
        try
        {
            var result = await _formulationService.CalculateFormulationAsync(productId, targetWeightKg);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
