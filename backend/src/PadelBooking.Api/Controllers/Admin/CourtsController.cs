using Microsoft.AspNetCore.Mvc;
using PadelBooking.Application.DTOs.Courts;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Api.Controllers.Admin;

[Route("api/admin/courts")]
public class CourtsController : AdminControllerBase
{
    private readonly ICourtService _courtService;

    public CourtsController(ICourtService courtService)
    {
        _courtService = courtService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CourtDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _courtService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _courtService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourtRequest request, CancellationToken cancellationToken)
    {
        var result = await _courtService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : HandleFailure(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourtRequest request, CancellationToken cancellationToken)
    {
        var result = await _courtService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _courtService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : HandleFailure(result);
    }
}
