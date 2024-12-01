using Microsoft.AspNetCore.Mvc;
using Zello.Application.Dtos;
using Zello.Application.Exceptions;
using Zello.Application.ServiceInterfaces;
using Zello.Infrastructure.Helpers;

/// <summary>
/// Controller for managing workspaces and their associated resources including members and projects
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class WorkspacesController : ControllerBase {
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService) {
        _workspaceService = workspaceService;
    }

    [HttpDelete("{workspaceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteWorkspace(Guid workspaceId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest(WorkspaceErrorMessages.UserIdMissing);

        try {
            await _workspaceService.DeleteWorkspaceAsync(workspaceId, userId.Value, userAccess);
            return NoContent();
        } catch (WorkspaceServiceException ex) {
            return ex switch {
                WorkspaceNotFoundException => NotFound(ex.Message),
                InsufficientPermissionsException => Forbid(ex.Message),
                _ => BadRequest(ex.Message)
            };
        }
    }

    [HttpPut("members/access")]
    [ProducesResponseType(typeof(WorkspaceMemberReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceMemberReadDto>> UpdateWorkspaceMemberAccess(
        [FromBody] MemberElevationDto elevation) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest(WorkspaceErrorMessages.UserIdMissing);

        try {
            var memberDto = new WorkspaceMemberCreateDto {
                UserId = userId.Value,
                AccessLevel = elevation.NewAccessLevel
            };

            return Ok(await _workspaceService.AddWorkspaceMemberAsync(elevation.MemberId, memberDto,
                userId.Value, userAccess));
        } catch (WorkspaceServiceException ex) {
            return ex switch {
                WorkspaceNotFoundException => NotFound(ex.Message),
                InsufficientPermissionsException => Forbid(ex.Message),
                _ => BadRequest(ex.Message)
            };
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceReadDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WorkspaceReadDto>>> GetAllWorkspaces() {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest(WorkspaceErrorMessages.UserIdMissing);

        try {
            var workspaces =
                await _workspaceService.GetAllWorkspacesAsync(userId.Value, userAccess);
            return Ok(workspaces);
        } catch (WorkspaceServiceException ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{workspaceId}")]
    [ProducesResponseType(typeof(WorkspaceReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceReadDto>> GetWorkspace(Guid workspaceId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest(WorkspaceErrorMessages.UserIdMissing);

        try {
            var workspace =
                await _workspaceService.GetWorkspaceByIdAsync(workspaceId, userId.Value,
                    userAccess);
            return Ok(workspace);
        } catch (WorkspaceServiceException ex) {
            return ex switch {
                WorkspaceNotFoundException => NotFound(ex.Message),
                InsufficientPermissionsException => Forbid(ex.Message),
                _ => BadRequest(ex.Message)
            };
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(WorkspaceReadDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<WorkspaceReadDto>> CreateWorkspace(
        [FromBody] WorkspaceCreateDto createWorkspace) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        if (userId == null) return BadRequest(WorkspaceErrorMessages.UserIdMissing);

        try {
            var workspace =
                await _workspaceService.CreateWorkspaceAsync(createWorkspace, userId.Value);
            return CreatedAtAction(nameof(GetWorkspace), new { workspaceId = workspace.Id },
                workspace);
        } catch (WorkspaceServiceException ex) {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{workspaceId}")]
    [ProducesResponseType(typeof(WorkspaceReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceReadDto>> UpdateWorkspace(
        Guid workspaceId,
        [FromBody] WorkspaceUpdateDto workspaceUpdateDto) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest(WorkspaceErrorMessages.UserIdMissing);

        try {
            var workspace = await _workspaceService.UpdateWorkspaceAsync(workspaceId,
                workspaceUpdateDto, userId.Value, userAccess);
            return Ok(workspace);
        } catch (WorkspaceServiceException ex) {
            return ex switch {
                WorkspaceNotFoundException => NotFound(ex.Message),
                InsufficientPermissionsException => Forbid(ex.Message),
                _ => BadRequest(ex.Message)
            };
        }
    }

    [HttpGet("{workspaceId}/members")]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceMemberReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<WorkspaceMemberReadDto>>> GetWorkspaceMembers(
        Guid workspaceId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest(WorkspaceErrorMessages.UserIdMissing);

        try {
            var members =
                await _workspaceService.GetWorkspaceMembersAsync(workspaceId, userId.Value,
                    userAccess);
            return Ok(members);
        } catch (WorkspaceServiceException ex) {
            return ex switch {
                WorkspaceNotFoundException => NotFound(ex.Message),
                InsufficientPermissionsException => Forbid(ex.Message),
                _ => BadRequest(ex.Message)
            };
        }
    }

    [HttpPost("{workspaceId}/members")]
    [ProducesResponseType(typeof(WorkspaceMemberReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceMemberReadDto>> AddWorkspaceMember(
        Guid workspaceId,
        [FromBody] WorkspaceMemberCreateDto createMember) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest(WorkspaceErrorMessages.UserIdMissing);

        try {
            var member = await _workspaceService.AddWorkspaceMemberAsync(workspaceId, createMember,
                userId.Value, userAccess);
            return CreatedAtAction(nameof(GetWorkspaceMembers), new { workspaceId }, member);
        } catch (WorkspaceServiceException ex) {
            return ex switch {
                WorkspaceNotFoundException => NotFound(ex.Message),
                InsufficientPermissionsException => Forbid(ex.Message),
                _ => BadRequest(ex.Message)
            };
        }
    }

    [HttpGet("{workspaceId}/projects")]
    [ProducesResponseType(typeof(IEnumerable<ProjectReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ProjectReadDto>>> GetWorkspaceProjects(
        Guid workspaceId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest(WorkspaceErrorMessages.UserIdMissing);

        try {
            var workspace =
                await _workspaceService.GetWorkspaceByIdAsync(workspaceId, userId.Value,
                    userAccess);
            return Ok(workspace.Projects);
        } catch (WorkspaceServiceException ex) {
            return ex switch {
                WorkspaceNotFoundException => NotFound(ex.Message),
                InsufficientPermissionsException => Forbid(ex.Message),
                _ => BadRequest(ex.Message)
            };
        }
    }

    [HttpPut("members/{memberId}/access")]
    [ProducesResponseType(typeof(WorkspaceMemberReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceMemberReadDto>> UpdateMemberAccess(
        Guid memberId,
        [FromBody] WorkspaceMemberUpdateDto updateDto) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest(WorkspaceErrorMessages.UserIdMissing);

        try {
            var member =
                await _workspaceService.UpdateMemberAccessAsync(memberId, updateDto, userId.Value,
                    userAccess);
            return Ok(member);
        } catch (WorkspaceServiceException ex) {
            return ex switch {
                WorkspaceNotFoundException => NotFound(ex.Message),
                WorkspaceMemberNotFoundException => NotFound(ex.Message),
                InsufficientPermissionsException => Forbid(ex.Message),
                _ => BadRequest(ex.Message)
            };
        }
    }
}
