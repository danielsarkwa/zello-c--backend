using Microsoft.EntityFrameworkCore;
using Zello.Application.Features.Authentication.Models;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.ServiceImplementations;

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
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Find user by username with minimal data
        var user = await _context.Set<User>()
            .AsNoTracking()  // Add this to prevent change tracking since we're just reading
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        var dbTime = sw.ElapsedMilliseconds;
        Console.WriteLine($"DB Query Time: {dbTime}ms");

        if (user == null) return null;

        var isValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        var hashTime = sw.ElapsedMilliseconds - dbTime;

        if (!isValid) return null;

        var token = _tokenService.GenerateToken(user);
        var jwtTime = sw.ElapsedMilliseconds - hashTime - dbTime;

        return new LoginResponse {
            Token = token,
            Expires = DateTime.UtcNow.AddHours(1),
            TokenType = "Bearer",
            AccessLevel = user.AccessLevel.ToString(),
            NumericLevel = (int)user.AccessLevel,
            Description = GetAccessLevelDescription(user.AccessLevel)
        };
    }

    public async Task<LoginResponse> ExtendSessionAsync(Guid userId) {
        var user = await _context.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        var token = _tokenService.GenerateToken(user);

        return new LoginResponse {
            Token = token,
            Expires = DateTime.UtcNow.AddHours(1),
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
