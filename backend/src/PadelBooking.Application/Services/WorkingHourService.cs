using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.WorkingHours;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Application.Services;

public class WorkingHourService : IWorkingHourService
{
    private readonly IAppDbContext _db;

    public WorkingHourService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<WorkingHourDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.WorkingHours
            .OrderBy(w => w.DayOfWeek)
            .Select(w => new WorkingHourDto
            {
                Id = w.Id,
                DayOfWeek = w.DayOfWeek,
                OpenTime = w.OpenTime,
                CloseTime = w.CloseTime,
                IsClosed = w.IsClosed
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<WorkingHourDto>> UpdateAsync(DayOfWeek dayOfWeek, UpdateWorkingHourRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.IsClosed && request.CloseTime <= request.OpenTime)
        {
            return Result<WorkingHourDto>.Failure("Close time must be after open time.");
        }

        var workingHour = await _db.WorkingHours.FirstOrDefaultAsync(w => w.DayOfWeek == dayOfWeek, cancellationToken);
        if (workingHour is null)
        {
            return Result<WorkingHourDto>.Failure($"No working-hour row exists for {dayOfWeek}.", ResultErrorType.NotFound);
        }

        workingHour.OpenTime = request.OpenTime;
        workingHour.CloseTime = request.CloseTime;
        workingHour.IsClosed = request.IsClosed;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<WorkingHourDto>.Success(new WorkingHourDto
        {
            Id = workingHour.Id,
            DayOfWeek = workingHour.DayOfWeek,
            OpenTime = workingHour.OpenTime,
            CloseTime = workingHour.CloseTime,
            IsClosed = workingHour.IsClosed
        });
    }
}
