using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Admin;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Application.Interfaces;

public interface IAdminBookingService
{
    Task<List<AdminBookingDto>> GetAllAsync(
        DateOnly? date = null,
        BookingStatus? status = null,
        int? courtId = null,
        PaymentMethod? paymentMethod = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<Result<AdminBookingDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<AdminBookingDto>> UpdateStatusAsync(int id, BookingStatus newStatus, CancellationToken cancellationToken = default);
}
