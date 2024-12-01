using System.Security.Claims;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.ServiceInterfaces;

public interface IAccessLevelService {
    AccessLevel? GetAccessLevel(ClaimsPrincipal user);
    bool HasRequiredAccessLevel(ClaimsPrincipal user, AccessLevel requiredLevel);
}
