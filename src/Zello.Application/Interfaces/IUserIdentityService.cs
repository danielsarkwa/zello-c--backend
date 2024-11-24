using System.Security.Claims;

namespace Zello.Application.Interfaces;

public interface IUserIdentityService {
    Guid? GetUserId(ClaimsPrincipal user);
    string? GetUsername(ClaimsPrincipal user);
}
