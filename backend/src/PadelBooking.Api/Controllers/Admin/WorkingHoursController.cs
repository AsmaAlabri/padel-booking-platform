using Microsoft.AspNetCore.Mvc;
using PadelBooking.Application.DTOs.WorkingHours;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Api.Controllers.Admin;

[Route("api/admin/working-hours")]
public class WorkingHoursController : AdminControllerBase
{
    private readonly IWorkingHourService _workingHourService;

    public WorkingHoursController(IWorkingHourService workingHourService)
    {
        _workingHourService = workingHourService;
    }

    [HttpGet]
    public async Task<ActionResult<List<WorkingHourDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _workingHourService.GetAllAsync(cancellationToken));

    /// <summary>Updates the hours for a single day of the week (0=Sunday .. 6=Saturday).</summary>
    [HttpPut("{dayOfWeek}")]
    public async Task<IActionResult> Update(DayOfWeek dayOfWeek, [FromBody] UpdateWorkingHourRequest request, CancellationToken cancellationToken)
    {
        var result = await _workingHourService.UpdateAsync(dayOfWeek, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
