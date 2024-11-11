using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class UserController : ControllerBase {
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult GetCurrentUser() {
        var response = new { Message = "Current user retrieved" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }
}
