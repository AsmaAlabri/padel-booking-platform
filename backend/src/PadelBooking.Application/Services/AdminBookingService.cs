using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Admin;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Entities;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Application.Services;

public class AdminBookingService : IAdminBookingService
{
    private readonly IAppDbContext _db;

    public AdminBookingService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AdminBookingDto>> GetAllAsync(
        DateOnly? date = null,
        BookingStatus? status = null,
        int? courtId = null,
        PaymentMethod? paymentMethod = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Bookings
            .Include(b => b.Court)
            .Include(b => b.Offer)
            .Include(b => b.Payment)
            .Include(b => b.Players)
            .AsQueryable();

        if (date is not null)
        {
            query = query.Where(b => b.BookingDate == date.Value);
        }

        if (status is not null)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        if (courtId is not null)
        {
            query = query.Where(b => b.CourtId == courtId.Value);
        }

        if (paymentMethod is not null)
        {
            query = query.Where(b => b.PaymentMethod == paymentMethod.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(b =>
                b.CustomerName.Contains(search) ||
                b.CustomerPhone.Contains(search) ||
                b.BookingReference.Contains(search));
        }

        var bookings = await query
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync(cancellationToken);

        bookings = bookings.OrderByDescending(b => b.BookingDate).ThenBy(b => b.StartTime).ToList();

        return bookings.Select(ToDto).ToList();
    }

    public async Task<Result<AdminBookingDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Court)
            .Include(b => b.Offer)
            .Include(b => b.Payment)
            .Include(b => b.Players)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (booking is null)
        {
            return Result<AdminBookingDto>.Failure("Booking not found.", ResultErrorType.NotFound);
        }

        return Result<AdminBookingDto>.Success(ToDto(booking));
    }

    public async Task<Result<AdminBookingDto>> UpdateStatusAsync(int id, BookingStatus newStatus, CancellationToken cancellationToken = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Court)
            .Include(b => b.Offer)
            .Include(b => b.Payment)
            .Include(b => b.Players)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (booking is null)
        {
            return Result<AdminBookingDto>.Failure("Booking not found.", ResultErrorType.NotFound);
        }

        if (newStatus == BookingStatus.Cancelled && booking.Status != BookingStatus.Cancelled)
        {
            var slots = await _db.BookingSlots
                .Where(s => s.BookingId == booking.Id)
                .ToListAsync(cancellationToken);
            _db.BookingSlots.RemoveRange(slots);
        }

        booking.Status = newStatus;
        await _db.SaveChangesAsync(cancellationToken);

        return Result<AdminBookingDto>.Success(ToDto(booking));
    }

    private static AdminBookingDto ToDto(Booking b) => new()
    {
        Id = b.Id,
        BookingReference = b.BookingReference,
        CustomerName = b.CustomerName,
        CustomerPhone = b.CustomerPhone,
        CustomerEmail = b.CustomerEmail,
        BookingDate = b.BookingDate,
        StartTime = b.StartTime,
        EndTime = b.EndTime,
        DurationHours = b.DurationHours,
        CourtId = b.CourtId,
        CourtName = b.Court?.Name ?? string.Empty,
        TotalPrice = b.TotalPrice,
        OfferApplied = b.Offer?.Name,
        PaymentMethod = b.PaymentMethod,
        PaymentStatus = b.Payment?.Status ?? PaymentStatus.NotRequired,
        Status = b.Status,
        CreatedAt = b.CreatedAt,
        PlayerNames = b.Players.Select(p => p.Name).ToList()
    };
}
