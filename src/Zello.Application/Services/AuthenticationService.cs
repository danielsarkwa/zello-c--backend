using Microsoft.EntityFrameworkCore;
using Zello.Application.Features.Authentication.Models;
using Zello.Application.Interfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Interfaces;

namespace Zello.Application.Services;

public class AuthenticationService : IAuthenticationService {
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly DbContext _context;

    public AuthenticationService(
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        DbContext context) {
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<LoginResponse?> AuthenticateUserAsync(TokenRequest request) {
        // Find user by username
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        // If user not found or password doesn't match, return null
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash)) {
            return null;
        }

        var token = _tokenService.GenerateToken(user);
        var expires = DateTime.UtcNow.AddHours(1);

        return new LoginResponse {
            Token = token,
            Expires = expires,
            TokenType = "Bearer",
            AccessLevel = user.AccessLevel.ToString(),
            NumericLevel = (int)user.AccessLevel,
            Description = GetAccessLevelDescription(user.AccessLevel)
        };
    }

    private static string GetAccessLevelDescription(AccessLevel level) => level switch {
        AccessLevel.Guest => "Basic access with limited privileges",
        AccessLevel.Member => "Standard user access with basic features",
        AccessLevel.Owner => "Elevated access with management capabilities",
        AccessLevel.Admin => "Full system access with all privileges",
        _ => "Unknown access level"
    };
}
