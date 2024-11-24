using Zello.Application.Features.Authentication.Models;
using Zello.Domain.Entities.Dto;

namespace Zello.Application.Interfaces;

public interface IAuthenticationService {
    LoginResponse? AuthenticateUser(string username, string password); // Remove async/Task
    string GenerateJwtToken(UserDto user);
}
