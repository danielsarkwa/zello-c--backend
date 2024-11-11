using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class ListController : ControllerBase {
    [HttpGet("{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult GetListById(string listId) {
        var response = new { Message = "List retrieved" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpPut("{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult UpdateList(string listId, [FromBody] object list) {
        var response = new { Message = "List updated" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpDelete("{listId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult DeleteList(string listId) {
        var response = new { Message = "List deleted" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpPost("{listId}/cards")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult CreateCard(string listId, [FromBody] object card) {
        var response = new { Message = "Card created" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }
}
