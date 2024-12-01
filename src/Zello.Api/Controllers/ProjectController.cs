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

            if (!hasAccess) return Forbid("User is not a member of the workspace");

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
