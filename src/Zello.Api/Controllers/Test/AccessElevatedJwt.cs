using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zello.Api.Authorization;
using Zello.Domain.Entities.Api.Message;
using Zello.Domain.Entities.Api.User;

namespace Zello.Api.Controllers.Test;

// Only for demonstration purposes 😉
#if DEBUG
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AccessElevatedJwt : ControllerBase {
    [HttpGet("guest")]
    [AllowAnonymous]
    public IActionResult GuestEndpoint() {
        return Ok(new SimpleMessage {
            Message = "This endpoint is accessible to everyone"
        });
    }

    [HttpGet("member")]
    [MinimumAccessLevel(AccessLevel.Member)]
    public IActionResult MemberEndpoint() {
        return Ok(new SimpleMessage {
            Message = "This endpoint requires Member or higher access"
        });
    }

    [HttpGet("owner")]
    [MinimumAccessLevel(AccessLevel.Owner)]
    public IActionResult OwnerEndpoint() {
        return Ok(new SimpleMessage {
            Message = "This endpoint requires Owner or higher access"
        });
    }

    [HttpGet("admin")]
    [MinimumAccessLevel(AccessLevel.Admin)]
    public IActionResult AdminEndpoint() {
        return Ok(new SimpleMessage {
            Message = "This endpoint requires Admin access"
        });
    }

    [HttpGet("whoami")]
    public IActionResult WhoAmI() {
        var username = User.Identity?.Name;
        var accessLevel = User.Claims
            .FirstOrDefault(c => c.Type == "AccessLevel")?.Value;
        var numericLevel = Enum.TryParse<AccessLevel>(accessLevel, out var level)
            ? (int)level
            : 0;

        return Ok(new {
            Username = username,
            AccessLevel = accessLevel,
            NumericLevel = numericLevel,
            CanAccessMember = numericLevel >= (int)AccessLevel.Member,
            CanAccessOwner = numericLevel >= (int)AccessLevel.Owner,
            CanAccessAdmin = numericLevel >= (int)AccessLevel.Admin
        });
    }
}
#endif
