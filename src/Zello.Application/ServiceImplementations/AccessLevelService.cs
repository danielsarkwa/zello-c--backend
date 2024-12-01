using System.Security.Claims;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Constants;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.ServiceImplementations;

public class AccessLevelService : IAccessLevelService {
    public AccessLevel? GetAccessLevel(ClaimsPrincipal user) {
        var accessLevelClaim = user.Claims
            .FirstOrDefault(c => c.Type == CustomClaimTypes.AccessLevel);

        if (accessLevelClaim != null &&
            Enum.TryParse<AccessLevel>(accessLevelClaim.Value, out var level)) {
            return level;
        }

        return null;
    }

    public bool HasRequiredAccessLevel(ClaimsPrincipal user, AccessLevel requiredLevel) {
        var userLevel = GetAccessLevel(user);
        return userLevel.HasValue && userLevel.Value >= requiredLevel;
    }
}
