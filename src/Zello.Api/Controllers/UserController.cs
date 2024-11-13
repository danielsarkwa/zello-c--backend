using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zello.Domain.Entities.Api.User;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UserController : ControllerBase {
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser() {
        try {
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username)) {
                return Unauthorized(new { Message = "User not authenticated" });
            }

            // Get this from the DB later
            var user = new UserDto {
                Email = username,
                Name = username,
                Username = username
            };

            return Ok(user);
        } catch (Exception ex) {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new {
                    SimpleMessage = "An error occurred while retrieving user data",
                    Reason = ex.Message
                });
        }
    }
}
