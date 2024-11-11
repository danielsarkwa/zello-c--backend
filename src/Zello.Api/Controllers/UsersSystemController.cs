using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/system/[controller]")]
public sealed class UsersSystemController : ControllerBase {
    [HttpPut("{userId}/role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult UpdateUserSystemRole(string userId, [FromBody] object roleUpdate) {
        var response = new { Message = "User system role updated" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }
}
