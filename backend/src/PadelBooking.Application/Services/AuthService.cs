using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Auth;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(IAppDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _db.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, cancellationToken);

        // Deliberately vague error message — never reveal whether the username exists.
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure("Invalid username or password.", ResultErrorType.Unauthorized);
        }

        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Username = user.Username,
            Role = user.Role.ToString()
        });
    }
}
