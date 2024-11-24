using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Zello.Application.Features.Authentication.Models;
using Zello.Application.Interfaces;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.TestingDataStorage;

namespace Zello.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService {
    private readonly IConfiguration _configuration;
    private readonly IAccessLevelService _accessLevelService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(
        IConfiguration configuration,
        IAccessLevelService accessLevelService,
        IPasswordHasher passwordHasher) {
        _configuration = configuration;
        _accessLevelService = accessLevelService;
        _passwordHasher = passwordHasher;
    }

    public LoginResponse? AuthenticateUser(string username, string password) {
        // Remove async/Task
        var user = TestData.FindUserByUsername(username);

        Console.WriteLine($"Looking for user: {username}");
        Console.WriteLine($"User found: {user != null}");

        if (user != null) {
            Console.WriteLine($"Stored password: {user.PasswordHash}");
            Console.WriteLine($"Provided password: {password}");
        }

        if (user == null || !_passwordHasher.VerifyPassword(password, user.PasswordHash)) {
            return null;
        }

        var token = GenerateJwtToken(user);

        return new LoginResponse {
            Token = token,
            Expires = DateTime.Now.AddHours(1),
            TokenType = "Bearer",
            AccessLevel = user.AccessLevel.ToString(),
            NumericLevel = (int)user.AccessLevel,
            Description = GetAccessLevelDescription(user.AccessLevel)
        };
    }

    public string GenerateJwtToken(UserDto user) {
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("AccessLevel", user.AccessLevel.ToString()),
            new Claim("UserId", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ??
            throw new InvalidOperationException("JWT Key not configured")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GetAccessLevelDescription(AccessLevel level) => level switch {
        AccessLevel.Guest => "Basic access with limited privileges",
        AccessLevel.Member => "Standard user access with basic features",
        AccessLevel.Owner => "Elevated access with management capabilities",
        AccessLevel.Admin => "Full system access with all privileges",
        _ => "Unknown access level"
    };
}
