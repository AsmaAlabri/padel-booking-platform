using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Auth;

namespace PadelBooking.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
