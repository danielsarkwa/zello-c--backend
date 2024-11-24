using Microsoft.AspNetCore.Mvc;
using Zello.Application.Features.Workspaces;
using Zello.Infrastructure.TestingDataStorage;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.Helpers;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class WorkspacesController : ControllerBase {
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<WorkspaceDto>
        CreateWorkspace([FromBody] CreateWorkspaceDto createWorkspace) {
        if (createWorkspace == null) return BadRequest();

        var workspace = createWorkspace.ToWorkspaceDto();
        workspace.Id = Guid.NewGuid();

        try {
            Guid? userId = ClaimsHelper.GetUserId(User);
            if (userId == null) {
                return BadRequest("User ID cannot be null.");
            }

            workspace.OwnerId = userId.Value;
        } catch (Exception e) {
            Console.WriteLine("Failed to get user ID from claims.", e);
            return StatusCode(500, "Internal server error.");
        }

        // Ensure collections are initialized
        workspace.Projects ??= new List<ProjectDto>();
        workspace.Members ??= new List<WorkspaceMemberDto>();

        TestData.TestWorkspaceCollection.Add(workspace.Id, workspace);

        return CreatedAtAction(nameof(GetWorkspace), new { workspaceId = workspace.Id }, workspace);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<WorkspaceDto>> GetAllWorkspaces() {
        return Ok(TestData.TestWorkspaceCollection.Values);
    }

    [HttpGet("{workspaceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<WorkspaceDto> GetWorkspace(Guid workspaceId) {
        if (!TestData.TestWorkspaceCollection.TryGetValue(workspaceId, out var workspace))
            return NotFound();

        return Ok(workspace);
    }

    [HttpPut("{workspaceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<WorkspaceDto> UpdateWorkspace(Guid workspaceId,
        [FromBody] UpdateWorkspaceDto updateWorkspace) {
        // Check if workspace exists and get reference
        if (!TestData.TestWorkspaceCollection.TryGetValue(workspaceId, out var existingWorkspace))
            return NotFound();

        updateWorkspace.UpdateWorkspace(existingWorkspace);


        return Ok(existingWorkspace);
    }

    [HttpDelete("{workspaceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteWorkspace(Guid workspaceId) {
        try {
            if (!TestData.TestWorkspaceCollection.ContainsKey(workspaceId))
                return NotFound();

            var deleted = TestData.DeleteWorkspace(workspaceId);
            if (!deleted) {
                return StatusCode(500, "Failed to delete workspace");
            }

            return NoContent();
        } catch (Exception ex) {
            return StatusCode(500, $"Failed to delete workspace: {ex.Message}");
        }
    }


    [HttpGet("{workspaceId}/projects")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<ProjectDto>> GetWorkspaceProjects(Guid workspaceId) {
        if (!TestData.TestWorkspaceCollection.ContainsKey(workspaceId))
            return NotFound();

        var projects = TestData.TestProjectCollection.Values
            .Where(p => p.WorkspaceId == workspaceId);
        return Ok(projects);
    }

    [HttpGet("{workspaceId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IEnumerable<WorkspaceMemberDto>> GetWorkspaceMembers(Guid workspaceId) {
        if (!TestData.TestWorkspaceCollection.ContainsKey(workspaceId))
            return NotFound();

        var members = TestData.TestWorkspaceMemberCollection.Values
            .Where(m => m.WorkspaceId == workspaceId);
        return Ok(members);
    }

    [HttpPost("{workspaceId}/members")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<WorkspaceMemberDto> AddWorkspaceMember(Guid workspaceId,
        [FromBody] CreateWorkspaceMemberDto createMember) {
        // Check if workspace exists
        if (!TestData.TestWorkspaceCollection.ContainsKey(workspaceId))
            return NotFound("Workspace not found");

        // Check if user exists
        if (!TestData.TestUserCollection.ContainsKey(createMember.UserId))
            return NotFound("User not found");

        // Create new workspace member
        var member = new WorkspaceMemberDto {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            UserId = createMember.UserId,
            AccessLevel = createMember.AccessLevel, // Set the access level
            CreatedDate = DateTime.UtcNow
        };

        TestData.TestWorkspaceMemberCollection.Add(member.Id, member);

        return CreatedAtAction(nameof(GetWorkspaceMembers), new { workspaceId }, member);
    }
}
