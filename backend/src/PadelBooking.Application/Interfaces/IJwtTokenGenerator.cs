using PadelBooking.Domain.Entities;

namespace PadelBooking.Application.Interfaces;

public interface IJwtTokenGenerator
{
    /// <summary>Generates a signed JWT for the given admin user. Returns the token string and its UTC expiry.</summary>
    (string Token, DateTime ExpiresAtUtc) GenerateToken(AdminUser adminUser);
}
