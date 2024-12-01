using System.Security.Claims;

namespace Zello.Application.ServiceInterfaces;

public interface IUserIdentityService {
    Guid? GetUserId(ClaimsPrincipal user);
    string? GetUsername(ClaimsPrincipal user);
}
