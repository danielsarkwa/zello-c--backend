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

    /// <summary>
    /// Deletes a workspace
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace to delete</param>
    /// <remarks>
    /// Required permissions:
    /// - Workspace Owner access level, or
    /// - Admin access
    /// </remarks>
    /// <response code="204">Workspace successfully deleted</response>
    /// <response code="400">User ID is null</response>
    /// <response code="403">Insufficient permissions to delete workspace</response>
    /// <response code="404">Workspace not found</response>
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

    /// <summary>
    /// Updates a workspace member's access level
    /// </summary>
    /// <param name="elevation">The member elevation details including member ID and new access level</param>
    /// <remarks>
    /// Required permissions:
    /// - Workspace Owner access level, or
    /// - Admin access
    ///
    /// Access level restrictions:
    /// - Cannot assign higher access than your own level (except admins)
    /// - Cannot modify access if you're below Owner level
    /// </remarks>
    /// <response code="200">Member access level successfully updated</response>
    /// <response code="400">Invalid elevation data or user ID is null</response>
    /// <response code="403">Insufficient permissions to modify access levels</response>
    /// <response code="404">Workspace member not found</response>
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

    /// <summary>
    /// Retrieves all workspaces accessible to the user
    /// </summary>
    /// <remarks>
    /// Returns:
    /// - All workspaces for Admin users
    /// - Only workspaces where the user is a member for non-Admin users
    /// </remarks>
    /// <response code="200">List of accessible workspaces</response>
    /// <response code="400">User ID is null</response>
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

    /// <summary>
    /// Retrieves a specific workspace by ID
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace</param>
    /// <remarks>
    /// Required permissions:
    /// - Workspace member access, or
    /// - Admin access
    /// </remarks>
    /// <response code="200">The requested workspace</response>
    /// <response code="400">User ID is null</response>
    /// <response code="403">Insufficient permissions to access workspace</response>
    /// <response code="404">Workspace not found</response>
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

    /// <summary>
    /// Creates a new workspace
    /// </summary>
    /// <param name="createWorkspace">Workspace creation details</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/Workspaces
    ///     {
    ///         "name": "New Workspace"
    ///     }
    ///
    /// The creator is automatically added as a workspace owner.
    /// </remarks>
    /// <response code="201">Workspace successfully created</response>
    /// <response code="400">Invalid workspace data or user ID is null</response>
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

    /// <summary>
    /// Updates an existing workspace
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace to update</param>
    /// <param name="workspaceUpdateDto">Updated workspace details</param>
    /// <remarks>
    /// Required permissions:
    /// - Workspace Owner access level, or
    /// - Admin access
    /// </remarks>
    /// <response code="200">Workspace successfully updated</response>
    /// <response code="400">Invalid update data or user ID is null</response>
    /// <response code="403">Insufficient permissions to update workspace</response>
    /// <response code="404">Workspace not found</response>
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

    /// <summary>
    /// Retrieves all members of a workspace
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace</param>
    /// <remarks>
    /// Required permissions:
    /// - Workspace member access, or
    /// - Admin access
    /// </remarks>
    /// <response code="200">List of workspace members</response>
    /// <response code="400">User ID is null</response>
    /// <response code="403">Insufficient permissions to view members</response>
    /// <response code="404">Workspace not found</response>
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

    /// <summary>
    /// Adds a new member to a workspace
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace</param>
    /// <param name="createMember">Member creation details</param>
    /// <remarks>
    /// Required permissions:
    /// - Workspace Owner access level, or
    /// - Admin access
    /// </remarks>
    /// <response code="201">Member successfully added to workspace</response>
    /// <response code="400">Invalid member data or user ID is null</response>
    /// <response code="403">Insufficient permissions to add members</response>
    /// <response code="404">Workspace not found</response>
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


    /// <summary>
    /// Retrieves all projects in a workspace
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace</param>
    /// <remarks>
    /// Required permissions:
    /// - Workspace member access, or
    /// - Admin access
    /// </remarks>
    /// <response code="200">List of workspace projects</response>
    /// <response code="400">User ID is null</response>
    /// <response code="403">Insufficient permissions to view projects</response>
    /// <response code="404">Workspace not found</response>
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

    /// <summary>
    /// Updates a member's access level within a workspace
    /// </summary>
    /// <param name="memberId">The unique identifier of the workspace member</param>
    /// <param name="updateDto">Updated access level details</param>
    /// <remarks>
    /// Required permissions:
    /// - Workspace Owner access level, or
    /// - Admin access
    ///
    /// Access level restrictions:
    /// - Cannot assign higher access than your own level (except admins)
    /// - Cannot modify access if you're below Owner level
    /// </remarks>
    /// <response code="200">Member access level successfully updated</response>
    /// <response code="400">Invalid update data or user ID is null</response>
    /// <response code="403">Insufficient permissions to modify access levels</response>
    /// <response code="404">Workspace or member not found</response>
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
