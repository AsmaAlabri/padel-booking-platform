using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Bookings;

namespace PadelBooking.Application.Interfaces;

public interface IBookingService
{
    Task<Result<BookingDto>> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);

    Task<Result<BookingDto>> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default);

    Task<Result<BookingDto>> CancelAsync(string bookingReference, CancellationToken cancellationToken = default);
}
