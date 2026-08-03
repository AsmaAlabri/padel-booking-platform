using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.WorkingHours;

namespace PadelBooking.Application.Interfaces;

public interface IWorkingHourService
{
    Task<List<WorkingHourDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<WorkingHourDto>> UpdateAsync(DayOfWeek dayOfWeek, UpdateWorkingHourRequest request, CancellationToken cancellationToken = default);
}
