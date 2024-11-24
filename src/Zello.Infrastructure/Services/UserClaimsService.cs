using System.Security.Claims;
using Zello.Application.Interfaces;
using Zello.Domain.Constants;
using Zello.Domain.Entities.Api.User;

namespace Zello.Infrastructure.Services;

public class UserClaimsService : IUserClaimsService {
    public AccessLevel? GetAccessLevel(ClaimsPrincipal user) {
        var accessLevelClaim = user.Claims
            .FirstOrDefault(c => c.Type == CustomClaimTypes.AccessLevel);

        if (accessLevelClaim != null &&
            Enum.TryParse<AccessLevel>(accessLevelClaim.Value, out var level)) {
            return level;
        }

        return null;
    }

    public Guid? GetUserId(ClaimsPrincipal user) {
        var userIdClaim = user.Claims
            .FirstOrDefault(c => c.Type == CustomClaimTypes.UserId);

        if (userIdClaim != null &&
            Guid.TryParse(userIdClaim.Value, out var userId)) {
            return userId;
        }

        return null;
    }
}
