using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase {
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult Register([FromBody] object user) {
        var response = new { Message = "User registered" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult Login([FromBody] object credentials) {
        var response = new { Message = "User logged in" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult RefreshToken([FromBody] object token) {
        var response = new { Message = "Token refreshed" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult Logout() {
        var response = new { Message = "User logged out" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpPost("password/change")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult ChangePassword([FromBody] object passwordChangeRequest) {
        var response = new { Message = "Password changed" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }
}
