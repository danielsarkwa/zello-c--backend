using Zello.Application.Features.Authentication.Models;

namespace Zello.Application.ServiceInterfaces;

public interface IAuthenticationService {
    Task<LoginResponse?> AuthenticateUserAsync(TokenRequest request);
    Task<LoginResponse> ExtendSessionAsync(Guid userId);
}
