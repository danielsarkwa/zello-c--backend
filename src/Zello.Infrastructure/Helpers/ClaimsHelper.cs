using System.Security.Claims;
using Zello.Domain.Entities.Api.User;

namespace Zello.Infrastructure.Helpers;

/// <summary>
/// Helper class for handling JWT claim operations
/// </summary>
public static class ClaimsHelper {
    /// <summary>
    /// Retrieves the user's access level from their claims
    /// </summary>
    /// <param name="user">The ClaimsPrincipal containing the user's claims</param>
    /// <returns>The user's AccessLevel if present and valid, null otherwise</returns>
    public static AccessLevel? GetUserAccessLevel(ClaimsPrincipal user) {
        var accessLevelClaim = user.Claims.FirstOrDefault(c => c.Type == "AccessLevel");
        if (accessLevelClaim != null &&
            Enum.TryParse<AccessLevel>(accessLevelClaim.Value, out var level)) {
            return level;
        }
        return null;
    }

    /// <summary>
    /// Retrieves the user's ID from their claims
    /// </summary>
    /// <param name="user">The ClaimsPrincipal containing the user's claims</param>
    /// <returns>The user's ID if present and valid, null otherwise</returns>
    public static Guid? GetUserId(ClaimsPrincipal user) {
        var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "UserId");
        if (userIdClaim != null &&
            Guid.TryParse(userIdClaim.Value, out var userId)) {
            return userId;
        }
        return null;
    }
}
