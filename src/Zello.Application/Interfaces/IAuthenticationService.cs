using Zello.Application.Features.Authentication.Models;
using Zello.Domain.Entities.Dto;

namespace Zello.Application.Interfaces;

public interface IAuthenticationService {
    Task<LoginResponse?> AuthenticateUserAsync(TokenRequest request);
}
