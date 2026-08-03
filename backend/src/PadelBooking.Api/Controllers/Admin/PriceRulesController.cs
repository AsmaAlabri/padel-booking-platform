using Microsoft.AspNetCore.Mvc;
using PadelBooking.Application.DTOs.PriceRules;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Api.Controllers.Admin;

[Route("api/admin/price-rules")]
public class PriceRulesController : AdminControllerBase
{
    private readonly IPriceRuleService _priceRuleService;

    public PriceRulesController(IPriceRuleService priceRuleService)
    {
        _priceRuleService = priceRuleService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PriceRuleDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _priceRuleService.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePriceRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _priceRuleService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePriceRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await _priceRuleService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _priceRuleService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
