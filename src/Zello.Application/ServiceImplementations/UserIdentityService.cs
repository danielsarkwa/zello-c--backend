using System.Security.Claims;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Constants;

namespace Zello.Application.ServiceImplementations;

public class UserIdentityService : IUserIdentityService {
    public Guid? GetUserId(ClaimsPrincipal user) {
        var userIdClaim = user.Claims
            .FirstOrDefault(c => c.Type == CustomClaimTypes.UserId);

        if (userIdClaim != null &&
            Guid.TryParse(userIdClaim.Value, out var userId)) {
            return userId;
        }

        return null;
    }

    public string? GetUsername(ClaimsPrincipal user) {
        return user.Identity?.Name;
    }
}
