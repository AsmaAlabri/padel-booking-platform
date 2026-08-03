using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Payments;

namespace PadelBooking.Application.Interfaces;

public interface IThawaniPaymentService
{
    Task<Result<ThawaniSessionResult>> CreateCheckoutSessionAsync(ThawaniCheckoutRequest request, CancellationToken cancellationToken = default);

    /// <summary>Server-to-server check of a session's true payment status — "paid", "unpaid", or "cancelled".</summary>
    Task<Result<string>> GetSessionPaymentStatusAsync(string sessionId, CancellationToken cancellationToken = default);
}
