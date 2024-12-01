using System.Security.Claims;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.ServiceInterfaces;

public interface IUserClaimsService {
    AccessLevel? GetAccessLevel(ClaimsPrincipal user);
    Guid? GetUserId(ClaimsPrincipal user);
}
