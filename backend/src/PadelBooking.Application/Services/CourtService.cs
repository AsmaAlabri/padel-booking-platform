using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Courts;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Application.Services;

public class CourtService : ICourtService
{
    private readonly IAppDbContext _db;

    public CourtService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CourtDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Courts
            .OrderBy(c => c.Name)
            .Select(c => ToDto(c))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<CourtDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var court = await _db.Courts.FindAsync(new object?[] { id }, cancellationToken);
        if (court is null)
        {
            return Result<CourtDto>.Failure($"Court with id {id} was not found.", ResultErrorType.NotFound);
        }

        return Result<CourtDto>.Success(ToDto(court));
    }

    public async Task<Result<CourtDto>> CreateAsync(CreateCourtRequest request, CancellationToken cancellationToken = default)
    {
        var nameExists = await _db.Courts.AnyAsync(c => c.Name == request.Name, cancellationToken);
        if (nameExists)
        {
            return Result<CourtDto>.Failure($"A court named '{request.Name}' already exists.", ResultErrorType.Conflict);
        }

        var court = new Court
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true
        };

        _db.Courts.Add(court);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CourtDto>.Success(ToDto(court));
    }

    public async Task<Result<CourtDto>> UpdateAsync(int id, UpdateCourtRequest request, CancellationToken cancellationToken = default)
    {
        var court = await _db.Courts.FindAsync(new object?[] { id }, cancellationToken);
        if (court is null)
        {
            return Result<CourtDto>.Failure($"Court with id {id} was not found.", ResultErrorType.NotFound);
        }

        var nameTaken = await _db.Courts.AnyAsync(c => c.Id != id && c.Name == request.Name, cancellationToken);
        if (nameTaken)
        {
            return Result<CourtDto>.Failure($"A court named '{request.Name}' already exists.", ResultErrorType.Conflict);
        }

        court.Name = request.Name;
        court.Description = request.Description;
        court.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<CourtDto>.Success(ToDto(court));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var court = await _db.Courts.FindAsync(new object?[] { id }, cancellationToken);
        if (court is null)
        {
            return Result<bool>.Failure($"Court with id {id} was not found.", ResultErrorType.NotFound);
        }

        var hasBookings = await _db.Bookings.AnyAsync(b => b.CourtId == id, cancellationToken);
        if (hasBookings)
        {
            return Result<bool>.Failure(
                "This court has existing bookings and cannot be deleted. Deactivate it instead.",
                ResultErrorType.Conflict);
        }

        _db.Courts.Remove(court);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private static CourtDto ToDto(Court court) => new()
    {
        Id = court.Id,
        Name = court.Name,
        Description = court.Description,
        IsActive = court.IsActive
    };
}
