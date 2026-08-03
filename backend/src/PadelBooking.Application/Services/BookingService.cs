using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Bookings;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Entities;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Application.Services;

public class BookingService : IBookingService
{
    private readonly IAppDbContext _db;
    private readonly IPricingService _pricingService;

    private const string ReferenceChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no O/0/I/1 — avoids visual ambiguity

    public BookingService(IAppDbContext db, IPricingService pricingService)
    {
        _db = db;
        _pricingService = pricingService;
    }

    public async Task<Result<BookingDto>> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);

        // ---------- Rule #11: no past, closed, or unavailable slots ----------

        if (request.Date < today)
        {
            return Result<BookingDto>.Failure("Cannot book a date in the past.");
        }

        if (request.Date == today && request.StartTime <= now.TimeOfDay)
        {
            return Result<BookingDto>.Failure("Cannot book a time slot that has already started or passed.");
        }

        var endTime = request.StartTime + TimeSpan.FromHours(request.DurationHours);

        var workingHour = await _db.WorkingHours
            .FirstOrDefaultAsync(w => w.DayOfWeek == request.Date.DayOfWeek, cancellationToken);

        if (workingHour is null || workingHour.IsClosed)
        {
            return Result<BookingDto>.Failure("The venue is closed on the selected date.");
        }

        if (request.StartTime < workingHour.OpenTime || endTime > workingHour.CloseTime)
        {
            return Result<BookingDto>.Failure("Selected time is outside working hours.");
        }

        // ---------- Rule #12: consecutive hours; Rule #2/#9: shared slot pool ----------

        var requestedHours = Enumerable.Range(0, request.DurationHours)
            .Select(i => request.StartTime + TimeSpan.FromHours(i))
            .ToList();

        var activeCourtIds = await _db.Courts
            .Where(c => c.IsActive)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (activeCourtIds.Count == 0)
        {
            return Result<BookingDto>.Failure("No courts are currently available for booking.", ResultErrorType.Conflict);
        }

        var closures = await _db.Closures
            .Where(c => c.Date == request.Date)
            .ToListAsync(cancellationToken);

        var existingSlots = await _db.BookingSlots
            .Where(s => s.SlotDate == request.Date && requestedHours.Contains(s.SlotHour))
            .ToListAsync(cancellationToken);

        var bookedByHour = existingSlots
            .GroupBy(s => s.SlotHour)
            .ToDictionary(g => g.Key, g => g.Select(s => s.CourtId).ToList());

        // A court must be free across EVERY requested hour, not just one — so we
        // intersect the free-court sets for each hour.
        List<int>? candidateCourtIds = null;
        foreach (var hour in requestedHours)
        {
            var bookedIds = bookedByHour.TryGetValue(hour, out var ids) ? ids : new List<int>();
            var freeForHour = CourtAvailabilityCalculator.GetAvailableCourtIds(
                activeCourtIds, closures, bookedIds, hour, hour + TimeSpan.FromHours(1));

            if (freeForHour.Count == 0)
            {
                return Result<BookingDto>.Failure(
                    "One or more of the requested hours is no longer available.", ResultErrorType.Conflict);
            }

            candidateCourtIds = candidateCourtIds is null
                ? freeForHour
                : candidateCourtIds.Intersect(freeForHour).ToList();
        }

        if (candidateCourtIds is null || candidateCourtIds.Count == 0)
        {
            return Result<BookingDto>.Failure(
                "No single court is free for the entire requested duration. Try a shorter duration or a different time.",
                ResultErrorType.Conflict);
        }

        // ---------- Pricing (Rule #14: prices + offers are admin-managed, applied server-side) ----------

        decimal subtotal = 0m;
        foreach (var hour in requestedHours)
        {
            subtotal += await _pricingService.GetHourlyPriceAsync(request.Date, hour, cancellationToken);
        }

        var offer = await _pricingService.GetApplicableOfferAsync(request.Date, cancellationToken);
        var discount = offer is null
            ? 0m
            : offer.DiscountType == DiscountType.Percentage
                ? subtotal * offer.DiscountValue / 100m
                : offer.DiscountValue;
        discount = Math.Min(discount, subtotal);
        var totalPrice = subtotal - discount;

        // ---------- Rule #10: random court assignment at confirmation, race-safe ----------

        Shuffle(candidateCourtIds);

        var bookingReference = await GenerateUniqueReferenceAsync(cancellationToken);

        foreach (var courtId in candidateCourtIds)
        {
            var initialStatus = request.PaymentMethod == PaymentMethod.PayOnArrival
                ? BookingStatus.Confirmed
                : BookingStatus.Pending; // held pending Thawani payment confirmation (Phase 4)

            var booking = new Booking
            {
                BookingReference = bookingReference,
                CustomerName = request.CustomerName ?? string.Empty,
                CustomerPhone = request.CustomerPhone,
                CustomerEmail = request.CustomerEmail,
                BookingDate = request.Date,
                StartTime = request.StartTime,
                EndTime = endTime,
                DurationHours = request.DurationHours,
                CourtId = courtId,
                PricePerHourSnapshot = subtotal / request.DurationHours,
                TotalPrice = totalPrice,
                OfferId = offer?.Id,
                PaymentMethod = request.PaymentMethod,
                Status = initialStatus
            };
            _db.Bookings.Add(booking);

            foreach (var hour in requestedHours)
            {
                _db.BookingSlots.Add(new BookingSlot
                {
                    Booking = booking,
                    CourtId = courtId,
                    SlotDate = request.Date,
                    SlotHour = hour
                });
            }

            _db.Payments.Add(new Payment
            {
                Booking = booking,
                Method = request.PaymentMethod,
                Status = request.PaymentMethod == PaymentMethod.PayOnArrival ? PaymentStatus.NotRequired : PaymentStatus.Pending,
                Amount = totalPrice
            });

            try
            {
                await _db.SaveChangesAsync(cancellationToken);

                return Result<BookingDto>.Success(new BookingDto
                {
                    BookingReference = booking.BookingReference,
                    CustomerName = booking.CustomerName,
                    CustomerPhone = booking.CustomerPhone,
                    CustomerEmail = booking.CustomerEmail,
                    BookingDate = booking.BookingDate,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    DurationHours = booking.DurationHours,
                    PricePerHourSnapshot = booking.PricePerHourSnapshot,
                    TotalPrice = booking.TotalPrice,
                    OfferApplied = offer?.Name,
                    PaymentMethod = booking.PaymentMethod,
                    Status = booking.Status,
                    CreatedAt = booking.CreatedAt
                });
            }
            catch (DbUpdateException)
            {
                // Someone else grabbed one of these court-hours between our check and
                // this insert (the unique index on BookingSlots rejected it). Clean
                // slate and try the next candidate court.
                _db.ClearTrackedEntities();
            }
        }

        return Result<BookingDto>.Failure(
            "This time slot was just booked by someone else. Please try a different time.",
            ResultErrorType.Conflict);
    }

    public async Task<Result<BookingDto>> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Offer)
            .FirstOrDefaultAsync(b => b.BookingReference == bookingReference, cancellationToken);

        if (booking is null)
        {
            return Result<BookingDto>.Failure("Booking not found.", ResultErrorType.NotFound);
        }

        return Result<BookingDto>.Success(MapToDto(booking));
    }

    public async Task<Result<BookingDto>> CancelAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        var booking = await _db.Bookings
            .Include(b => b.Offer)
            .FirstOrDefaultAsync(b => b.BookingReference == bookingReference, cancellationToken);

        if (booking is null)
        {
            return Result<BookingDto>.Failure("Booking not found.", ResultErrorType.NotFound);
        }

        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed or BookingStatus.NoShow or BookingStatus.Expired)
        {
            return Result<BookingDto>.Failure("This booking can no longer be cancelled.", ResultErrorType.Conflict);
        }

        var bookingStart = booking.BookingDate.ToDateTime(TimeOnly.FromTimeSpan(booking.StartTime));
        if (bookingStart <= DateTime.Now)
        {
            return Result<BookingDto>.Failure("This booking has already started and can no longer be cancelled.", ResultErrorType.Conflict);
        }

        booking.Status = BookingStatus.Cancelled;

        var slots = await _db.BookingSlots
            .Where(s => s.BookingId == booking.Id)
            .ToListAsync(cancellationToken);
        _db.BookingSlots.RemoveRange(slots);

        await _db.SaveChangesAsync(cancellationToken);

        return Result<BookingDto>.Success(MapToDto(booking));
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        BookingReference = b.BookingReference,
        CustomerName = b.CustomerName,
        CustomerPhone = b.CustomerPhone,
        CustomerEmail = b.CustomerEmail,
        BookingDate = b.BookingDate,
        StartTime = b.StartTime,
        EndTime = b.EndTime,
        DurationHours = b.DurationHours,
        PricePerHourSnapshot = b.PricePerHourSnapshot,
        TotalPrice = b.TotalPrice,
        OfferApplied = b.Offer?.Name,
        PaymentMethod = b.PaymentMethod,
        Status = b.Status,
        CreatedAt = b.CreatedAt
    };

    private async Task<string> GenerateUniqueReferenceAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var candidate = GenerateReference();
            var exists = await _db.Bookings.AnyAsync(b => b.BookingReference == candidate, cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Failed to generate a unique booking reference after multiple attempts.");
    }

    private static string GenerateReference()
    {
        var bytes = RandomNumberGenerator.GetBytes(6);
        var sb = new StringBuilder("PB-");
        foreach (var b in bytes)
        {
            sb.Append(ReferenceChars[b % ReferenceChars.Length]);
        }
        return sb.ToString();
    }

    private static void Shuffle(IList<int> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
