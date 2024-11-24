using System.Security.Claims;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Interfaces;

public interface IAccessLevelService {
    AccessLevel? GetAccessLevel(ClaimsPrincipal user);
    bool HasRequiredAccessLevel(ClaimsPrincipal user, AccessLevel requiredLevel);
}
