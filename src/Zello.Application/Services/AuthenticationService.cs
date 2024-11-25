using Zello.Application.Features.Authentication.Models;
using Zello.Application.Interfaces;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Interfaces;

namespace Zello.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService {
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUserRepository _userRepository;

    public AuthenticationService(
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUserRepository userRepository) {
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _userRepository = userRepository;
    }

    public LoginResponse? AuthenticateUser(TokenRequest request) {
        var user = _userRepository.FindByUsername(request.Username);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash)) {
            return null;
        }

        var token = _tokenService.GenerateToken(user);
        var expires = DateTime.Now.AddHours(1);

        return new LoginResponse {
            Token = token,
            Expires = expires,
            TokenType = "Bearer",
            AccessLevel = user.AccessLevel.ToString(),
            NumericLevel = (int)user.AccessLevel,
            Description = GetAccessLevelDescription(user.AccessLevel)
        };
    }

    private string GetAccessLevelDescription(AccessLevel level) => level switch {
        AccessLevel.Guest => "Basic access with limited privileges",
        AccessLevel.Member => "Standard user access with basic features",
        AccessLevel.Owner => "Elevated access with management capabilities",
        AccessLevel.Admin => "Full system access with all privileges",
        _ => "Unknown access level"
    };
}
