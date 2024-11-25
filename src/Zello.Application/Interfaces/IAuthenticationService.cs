using Zello.Application.Features.Authentication.Models;
using Zello.Domain.Entities.Dto;

namespace Zello.Application.Interfaces;

public interface IAuthenticationService {
    LoginResponse? AuthenticateUser(TokenRequest request); // Remove async/Task
}
