using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class ProjectController : ControllerBase {
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult CreateProject([FromBody] object project) {
        var response = new { Message = "Project created" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpGet("{projectId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult GetProjectById(string projectId) {
        var response = new { Message = "Project retrieved" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpPut("{projectId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult UpdateProject(string projectId, [FromBody] object project) {
        var response = new { Message = "Project updated" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpDelete("{projectId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult DeleteProject(string projectId) {
        var response = new { Message = "Project deleted" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpPost("{projectId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult AddProjectMember(string projectId, [FromBody] object member) {
        var response = new { Message = "Member added to project" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }

    [HttpPost("{projectId}/lists")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ContentResult CreateList(string projectId, [FromBody] object list) {
        var response = new { Message = "List created" };
        return new ContentResult {
            Content = JsonConvert.SerializeObject(response),
            ContentType = "application/json",
            StatusCode = StatusCodes.Status200OK
        };
    }
}
