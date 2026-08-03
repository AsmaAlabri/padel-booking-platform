using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Closures;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Application.Services;

public class ClosureService : IClosureService
{
    private readonly IAppDbContext _db;

    public ClosureService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClosureDto>> GetAllAsync(DateOnly? fromDate = null, CancellationToken cancellationToken = default)
    {
        var query = _db.Closures.Include(c => c.Court).AsQueryable();

        if (fromDate is not null)
        {
            query = query.Where(c => c.Date >= fromDate.Value);
        }

        return await query
            .OrderBy(c => c.Date)
            .Select(c => new ClosureDto
            {
                Id = c.Id,
                CourtId = c.CourtId,
                CourtName = c.Court != null ? c.Court.Name : null,
                Date = c.Date,
                StartTime = c.StartTime,
                EndTime = c.EndTime,
                Reason = c.Reason
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<ClosureDto>> CreateAsync(CreateClosureRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CourtId is not null)
        {
            var courtExists = await _db.Courts.AnyAsync(c => c.Id == request.CourtId, cancellationToken);
            if (!courtExists)
            {
                return Result<ClosureDto>.Failure($"Court with id {request.CourtId} was not found.", ResultErrorType.NotFound);
            }
        }

        if ((request.StartTime is null) != (request.EndTime is null))
        {
            return Result<ClosureDto>.Failure("StartTime and EndTime must both be set, or both left empty for a full-day closure.");
        }

        if (request.StartTime is not null && request.EndTime is not null && request.EndTime <= request.StartTime)
        {
            return Result<ClosureDto>.Failure("EndTime must be after StartTime.");
        }

        var closure = new Closure
        {
            CourtId = request.CourtId,
            Date = request.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Reason = request.Reason
        };

        _db.Closures.Add(closure);
        await _db.SaveChangesAsync(cancellationToken);

        var courtName = request.CourtId is not null
            ? (await _db.Courts.FindAsync(new object?[] { request.CourtId }, cancellationToken))?.Name
            : null;

        return Result<ClosureDto>.Success(new ClosureDto
        {
            Id = closure.Id,
            CourtId = closure.CourtId,
            CourtName = courtName,
            Date = closure.Date,
            StartTime = closure.StartTime,
            EndTime = closure.EndTime,
            Reason = closure.Reason
        });
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var closure = await _db.Closures.FindAsync(new object?[] { id }, cancellationToken);
        if (closure is null)
        {
            return Result<bool>.Failure($"Closure with id {id} was not found.", ResultErrorType.NotFound);
        }

        _db.Closures.Remove(closure);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
