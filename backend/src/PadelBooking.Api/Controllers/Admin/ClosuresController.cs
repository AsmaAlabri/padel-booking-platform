using Microsoft.AspNetCore.Mvc;
using PadelBooking.Application.DTOs.Closures;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Api.Controllers.Admin;

[Route("api/admin/closures")]
public class ClosuresController : AdminControllerBase
{
    private readonly IClosureService _closureService;

    public ClosuresController(IClosureService closureService)
    {
        _closureService = closureService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClosureDto>>> GetAll([FromQuery] DateOnly? fromDate, CancellationToken cancellationToken) =>
        Ok(await _closureService.GetAllAsync(fromDate, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClosureRequest request, CancellationToken cancellationToken)
    {
        var result = await _closureService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _closureService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
