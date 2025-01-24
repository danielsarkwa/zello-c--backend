using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Application.Dtos;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Helpers;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class ProjectController : ControllerBase {
    private readonly IProjectService _projectService;
    private readonly IAuthorizationService _authorizationService;

    public ProjectController(IProjectService projectService,
        IAuthorizationService authorizationService) {
        _projectService = projectService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Creates a new project
    /// </summary>
    /// <param name="projectDto">Project creation details</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/Project
    ///     {
    ///         "name": "New Project",
    ///         "description": "Project description",
    ///         "workspace_id": "123e4567-e89b-12d3-a456-426614174000",
    ///         "start_date": "2024-01-01",
    ///         "end_date": "2024-12-31",
    ///         "status": "NotStarted"
    ///     }
    ///
    /// Required permissions:
    /// - Workspace member access
    /// </remarks>
    /// <response code="201">Project successfully created</response>
    /// <response code="400">Invalid project data or user ID is null</response>
    /// <response code="403">User is not a member of the workspace</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProject([FromBody] ProjectCreateDto projectDto) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try {
            var userId = ClaimsHelper.GetUserId(User);
            var userAccess = ClaimsHelper.GetUserAccessLevel(User);
            if (userId == null) return BadRequest("User ID cannot be null.");

            // Verify user has appropriate access in the workspace
            var hasAccess = await _authorizationService.AuthorizeWorkspaceMembershipAsync(
                projectDto.WorkspaceId, userId.Value);

            if (!hasAccess)
                return new ObjectResult(new { Message = "User is not a member of the workspace" }) {
                    StatusCode = StatusCodes.Status403Forbidden
                };

            // check if the user has suffient permissions to create a project
            var hasPermission = await _authorizationService.AuthorizeProjectAccessAsync(
                userId.Value, projectDto.WorkspaceId, AccessLevel.Member);

            var project = await _projectService.CreateProjectAsync(projectDto, userId.Value);

            return CreatedAtAction(nameof(GetProjectById), new { projectId = project.Id }, project);
        } catch (Exception e) {
            return StatusCode(500, "Internal server error: " + e.Message);
        }
    }

    /// <summary>
    /// Updates the access level of a project member
    /// </summary>
    /// <param name="elevation">The member elevation details including member ID and new access level</param>
    /// <remarks>
    /// Requires the following permissions:
    /// - Admin access, or
    /// - Project Owner access level
    ///
    /// Access level restrictions:
    /// - Cannot assign higher access than your own level (except admins)
    /// - Cannot exceed member's workspace access level
    /// - Cannot modify access if you're below Owner level
    /// </remarks>
    /// <response code="200">Member access level successfully updated</response>
    /// <response code="400">Invalid elevation data provided</response>
    /// <response code="403">Insufficient permissions to modify access levels</response>
    /// <response code="404">Project member not found</response>
    [HttpPut("members/access")]
    [ProducesResponseType(typeof(ProjectMemberReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProjectMemberAccess(
        [FromBody] MemberElevationDto elevation) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User) ?? AccessLevel.Guest;
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var projectMember = await _projectService.UpdateMemberAccessAsync(
                elevation.MemberId,
                elevation.NewAccessLevel,
                userId.Value,
                userAccess);

            return Ok(ProjectMemberReadDto.FromEntity(projectMember));
        } catch (UnauthorizedAccessException ex) {
            return Forbid(ex.Message);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
    }


    /// <summary>
    /// Retrieves a project by its ID
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <remarks>
    /// Required permissions:
    /// - Project member access, or
    /// - Admin access
    /// </remarks>
    /// <response code="200">Returns the requested project</response>
    /// <response code="400">User ID is null</response>
    /// <response code="403">User does not have access to this project</response>
    /// <response code="404">Project not found</response>
    [HttpGet("{projectId}")]
    [ProducesResponseType(typeof(ProjectReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProjectById(Guid projectId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var project = await _projectService.GetProjectByIdAsync(projectId);

            if (project == null)
                return NotFound($"Project with ID {projectId} not found");

            // verfrify user has access to the project
            var hasAccess =
                await _authorizationService.AuthorizeProjectMembershipAsync(userId.Value,
                    projectId);

            if (!hasAccess) return Forbid("User does not have access to this project");

            return Ok(project);
        } catch (KeyNotFoundException) {
            return NotFound($"Project with ID {projectId} not found");
        }
    }

    /// <summary>
    /// Retrieves all projects accessible to the user
    /// </summary>
    /// <param name="workspaceId">Optional workspace ID to filter projects</param>
    /// <remarks>
    /// For non-admin users, returns only projects where they are a member.
    /// Admin users can see all projects.
    ///
    /// Optional query parameter:
    /// - workspaceId: Filter projects by workspace
    /// </remarks>
    /// <response code="200">Returns list of accessible projects</response>
    /// <response code="400">User ID is null</response>
    /// <response code="403">Insufficient permissions</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllProjects([FromQuery] Guid? workspaceId = null) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        var projects = await _projectService.GetAllProjectsAsync(workspaceId);

        // Filter to only show projects where user is a member or is admin
        if (userAccess != AccessLevel.Admin) {
            projects = projects.Where(p =>
                p.Members.Any(pm =>
                    pm.WorkspaceMember.UserId == userId
                )
            ).ToList();
        }

        return Ok(projects);
    }

    /// <summary>
    /// Updates an existing project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project to update</param>
    /// <param name="updatedProject">Updated project details</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/v1/Project/{projectId}
    ///     {
    ///         "name": "Updated Name",
    ///         "description": "Updated description",
    ///         "startDate": "2024-02-01",
    ///         "endDate": "2024-12-31",
    ///         "status": "InProgress"
    ///     }
    ///
    /// Required permissions:
    /// - Project Owner access level, or
    /// - Admin access
    /// </remarks>
    /// <response code="200">Project successfully updated</response>
    /// <response code="400">Invalid update data or user ID is null</response>
    /// <response code="403">Insufficient permissions to update project</response>
    /// <response code="404">Project not found</response>
    [HttpPut("{projectId}")]
    [ProducesResponseType(typeof(ProjectReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateProject(Guid projectId,
        [FromBody] ProjectUpdateDto updatedProject) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var project = await _projectService.UpdateProjectAsync(projectId, updatedProject);

            if (project == null)
                return NotFound($"Project with ID {projectId} not found");

            // Check if current user has access to manage members
            var currentProjectMember = project.Members
                .FirstOrDefault(pm => pm.WorkspaceMember != null &&
                                      pm.WorkspaceMember.UserId == userId);

            bool hasAccess = userAccess == AccessLevel.Admin ||
                             (currentProjectMember?.AccessLevel >= AccessLevel.Owner);

            if (!hasAccess)
                return Forbid("Insufficient permissions to update project");

            return Ok(project);
        } catch (Exception e) {
            return StatusCode(500, "Internal server error: " + e.Message);
        }
    }

    /// <summary>
    /// Deletes a project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project to delete</param>
    /// <remarks>
    /// Required permissions:
    /// - Project Owner access level, or
    /// - Admin access
    /// </remarks>
    /// <response code="204">Project successfully deleted</response>
    /// <response code="400">User ID is null</response>
    /// <response code="403">Insufficient permissions to delete project</response>
    /// <response code="404">Project not found</response>
    [HttpDelete("{projectId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteProject(Guid projectId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var project = await _projectService.GetProjectByIdAsync(projectId);

            // Check if current user has access to manage members
            var currentProjectMember = project.Members
                .FirstOrDefault(pm => pm.WorkspaceMember != null &&
                                      pm.WorkspaceMember.UserId == userId);

            bool hasAccess = userAccess == AccessLevel.Admin ||
                             (currentProjectMember?.AccessLevel >= AccessLevel.Owner);

            if (!hasAccess)
                return Forbid("Insufficient permissions to update project");

            await _projectService.DeleteProjectAsync(projectId);
            return NoContent();
        } catch (KeyNotFoundException) {
            return NotFound($"Project with ID {projectId} not found");
        }
    }

    /// <summary>
    /// Adds a new member to a project
    /// </summary>
    /// <param name="createMember">Project member details</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/Project/members
    ///     {
    ///         "project_id": "123e4567-e89b-12d3-a456-426614174000",
    ///         "workspace_member_id": "123e4567-e89b-12d3-a456-426614174001",
    ///         "access_level": "Member"
    ///     }
    ///
    /// Required permissions:
    /// - Project Owner access level, or
    /// - Admin access
    /// </remarks>
    /// <response code="201">Member successfully added to project</response>
    /// <response code="400">Invalid member data or user ID is null</response>
    /// <response code="403">Insufficient permissions to add members</response>
    /// <response code="404">Project or workspace member not found</response>
    [HttpPost("members")]
    [ProducesResponseType(typeof(ProjectMemberReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddProjectMember(
        [FromBody] ProjectMemberCreateDto createMember) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var projectMember =
                await _projectService.AddProjectMemberAsync(createMember, userId.Value);
            return CreatedAtAction(nameof(GetProjectById),
                new { projectId = createMember.ProjectId },
                ProjectMemberReadDto.FromEntity(projectMember));
        } catch (UnauthorizedAccessException ex) {
            return Forbid(ex.Message);
        } catch (InvalidOperationException ex) {
            return BadRequest(ex.Message);
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new list in a project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <param name="model">List creation details</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/Project/{projectId}/lists
    ///     {
    ///         "name": "New List",
    ///         "tasks": [
    ///             {
    ///                 "name": "Task 1",
    ///                 "description": "Task description",
    ///                 "status": "ToDo",
    ///                 "priority": "Medium"
    ///             }
    ///         ]
    ///     }
    ///
    /// Required permissions:
    /// - Project member access
    /// </remarks>
    /// <response code="201">List successfully created</response>
    /// <response code="400">Invalid list data or user ID is null</response>
    /// <response code="403">User is not a project member</response>
    /// <response code="404">Project not found</response>
    [HttpPost("{projectId}/lists")]
    [ProducesResponseType(typeof(ListReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateList(Guid projectId, [FromBody] ListCreateDto model) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        // Check authorization first
        var isAuthorized =
            await _authorizationService.AuthorizeProjectMembershipAsync(userId.Value, projectId);
        if (!isAuthorized) return Forbid();

        try {
            var list = await _projectService.CreateListAsync(projectId, model);
            var dto = ListReadDto.FromEntity(list);
            return CreatedAtAction(
                nameof(GetProjectById),
                new { projectId },
                dto
            );
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all lists in a project
    /// </summary>
    /// <param name="projectId">The unique identifier of the project</param>
    /// <remarks>
    /// Required permissions:
    /// - Project member access
    /// </remarks>
    /// <response code="200">Returns the project's lists</response>
    /// <response code="400">User ID is null</response>
    /// <response code="403">User does not have access to this project</response>
    /// <response code="404">Project not found</response>
    [HttpGet("{projectId}/lists")]
    [ProducesResponseType(typeof(IEnumerable<ListReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProjectLists(Guid projectId) {
        var userId = ClaimsHelper.GetUserId(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        try {
            var lists = await _projectService.GetProjectListsAsync(projectId);
            return Ok(lists.Select(ListReadDto.FromEntity));
        } catch (KeyNotFoundException ex) {
            return NotFound(ex.Message);
        } catch (UnauthorizedAccessException) {
            return Forbid("User does not have access to this project");
        }
    }
}
