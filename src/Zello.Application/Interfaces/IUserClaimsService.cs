using System.Security.Claims;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Interfaces;

public interface IUserClaimsService {
    AccessLevel? GetAccessLevel(ClaimsPrincipal user);
    Guid? GetUserId(ClaimsPrincipal user);
}
