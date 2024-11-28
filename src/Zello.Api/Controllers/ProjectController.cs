using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Helpers;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class ProjectController : ControllerBase {
    private readonly ApplicationDbContext _context;

    public ProjectController(ApplicationDbContext context) {
        _context = context;
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

            // Verify workspace exists and user has appropriate access
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .FirstOrDefaultAsync(w => w.Id == projectDto.WorkspaceId);

            if (workspace == null)
                return NotFound("Invalid workspace ID");

            var workspaceMember = workspace.Members
                .FirstOrDefault(m => m.UserId == userId.Value);

            if (workspaceMember == null && userAccess != AccessLevel.Admin)
                return Forbid("User is not a member of the workspace");

            // Only workspace members with Member access or above can create projects
            if (workspaceMember?.AccessLevel < AccessLevel.Member &&
                userAccess != AccessLevel.Admin)
                return Forbid("Insufficient permissions to create projects");

            var project = projectDto.ToEntity();

            var projectMember = new ProjectMember {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                WorkspaceMemberId = workspaceMember!.Id,
                AccessLevel = AccessLevel.Owner,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Projects.AddAsync(project);
            await _context.ProjectMembers.AddAsync(projectMember);
            await _context.SaveChangesAsync();

            var createdProject = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            return CreatedAtAction(nameof(GetProjectById),
                new { projectId = project.Id },
                ProjectReadDto.FromEntity(createdProject!));
        } catch (Exception e) {
            return StatusCode(500, "Internal server error: " + e.Message);
        }
    }

    // In ProjectController.cs
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
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        var projectMember = await _context.ProjectMembers
            .Include(pm => pm.Project)
            .ThenInclude(p => p.Members)
            .ThenInclude(m => m.WorkspaceMember)
            .Include(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(pm => pm.Id == elevation.MemberId);

        if (projectMember == null)
            return NotFound("Project member not found");

        // Check if current user has access to manage members
        var currentProjectMember = projectMember.Project.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         (currentProjectMember?.AccessLevel >= AccessLevel.Owner);

        if (!hasAccess)
            return Forbid("Insufficient permissions to manage member access levels");

        // Cannot assign higher access than own level
        if (currentProjectMember != null &&
            elevation.NewAccessLevel > currentProjectMember.AccessLevel &&
            userAccess != AccessLevel.Admin)
            return Forbid("Cannot assign access level higher than your own");

        // Cannot exceed workspace access level
        if (elevation.NewAccessLevel > projectMember.WorkspaceMember.AccessLevel)
            return Forbid(
                "Cannot assign project access level higher than user's workspace access level");

        projectMember.AccessLevel = elevation.NewAccessLevel;
        await _context.SaveChangesAsync();

        return Ok(ProjectMemberReadDto.FromEntity(projectMember));
    }


    [HttpGet("{projectId}")]
    [ProducesResponseType(typeof(ProjectReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProjectById(Guid projectId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        var project = await _context.Projects
            .Include(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .Include(p => p.Lists)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound($"Project with ID {projectId} not found");

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         project.Members.Any(pm =>
                             pm.WorkspaceMember.UserId == userId
                         );

        if (!hasAccess)
            return Forbid("User does not have access to this project");

        return Ok(ProjectReadDto.FromEntity(project));
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllProjects([FromQuery] Guid? workspaceId = null) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        var query = _context.Projects
            .Include(p => p.Lists)
            .ThenInclude(l => l.Tasks)
            .Include(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .AsQueryable();

        if (workspaceId.HasValue)
            query = query.Where(p => p.WorkspaceId == workspaceId.Value);

        // Filter to only show projects where user is a member or is admin
        if (userAccess != AccessLevel.Admin) {
            query = query.Where(p =>
                p.Members.Any(pm =>
                    pm.WorkspaceMember.UserId == userId
                )
            );
        }

        var projects = await query.ToListAsync();
        return Ok(projects.Select(ProjectReadDto.FromEntity));
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
            var project = await _context.Projects
                .Include(p => p.Members)
                .ThenInclude(pm => pm.WorkspaceMember)
                .Include(p => p.Lists)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return NotFound($"Project with ID {projectId} not found");

            var projectMember = project.Members
                .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

            bool hasAccess = userAccess == AccessLevel.Admin ||
                             (projectMember?.AccessLevel >= AccessLevel.Member);

            if (!hasAccess)
                return Forbid("Insufficient permissions to update project");

            project = updatedProject.ToEntity(project);
            await _context.SaveChangesAsync();

            return Ok(ProjectReadDto.FromEntity(project));
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

        var project = await _context.Projects
            .Include(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .Include(p => p.Lists)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound($"Project with ID {projectId} not found");

        var projectMember = project.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         (projectMember?.AccessLevel >= AccessLevel.Owner);

        if (!hasAccess)
            return Forbid("Insufficient permissions to delete project");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("members")]
    [ProducesResponseType(typeof(ProjectMemberReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddProjectMember(
        [FromBody] ProjectMemberCreateDto createMember) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        var project = await _context.Projects
            .Include(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(p => p.Id == createMember.ProjectId);

        if (project == null)
            return NotFound($"Project with ID {createMember.ProjectId} not found");

        // Check if current user has access to manage members
        var currentProjectMember = project.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         (currentProjectMember?.AccessLevel >= AccessLevel.Member);

        if (!hasAccess)
            return Forbid("Insufficient permissions to manage project members");

        // Validate target workspace member
        var targetWorkspaceMember = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(wm => wm.Id == createMember.WorkspaceMemberId);

        if (targetWorkspaceMember == null)
            return BadRequest("Invalid workspace member ID");

        if (targetWorkspaceMember.WorkspaceId != project.WorkspaceId)
            return BadRequest("Workspace member does not belong to the project's workspace");

        // Cannot assign higher access than own level
        if (currentProjectMember != null &&
            createMember.AccessLevel > currentProjectMember.AccessLevel &&
            userAccess != AccessLevel.Admin)
            return Forbid("Cannot assign access level higher than your own");

        // Cannot exceed workspace access level
        if (createMember.AccessLevel > targetWorkspaceMember.AccessLevel)
            return Forbid(
                "Cannot assign project access level higher than user's workspace access level");

        if (project.Members.Any(m => m.WorkspaceMemberId == createMember.WorkspaceMemberId))
            return BadRequest("Member already exists in project");

        var projectMember = new ProjectMember {
            Id = Guid.NewGuid(),
            ProjectId = createMember.ProjectId,
            WorkspaceMemberId = createMember.WorkspaceMemberId,
            AccessLevel = createMember.AccessLevel,
            CreatedDate = DateTime.UtcNow
        };

        await _context.ProjectMembers.AddAsync(projectMember);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProjectById),
            new { projectId = createMember.ProjectId },
            ProjectMemberReadDto.FromEntity(projectMember));
    }

    [HttpPost("{projectId}/lists")]
    [ProducesResponseType(typeof(ListReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateList(Guid projectId, [FromBody] ListCreateDto listDto) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        var project = await _context.Projects
            .Include(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound($"Project with ID {projectId} not found");

        var projectMember = project.Members
            .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         (projectMember?.AccessLevel >= AccessLevel.Member);

        if (!hasAccess)
            return Forbid("Insufficient permissions to create lists");

        var maxPosition = await _context.Lists
            .Where(l => l.ProjectId == projectId)
            .MaxAsync(l => (int?)l.Position) ?? -1;

        var list = new TaskList {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = listDto.Name,
            Position = maxPosition + 1,
            CreatedDate = DateTime.UtcNow
        };

        await _context.Lists.AddAsync(list);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProjectById),
            new { projectId },
            ListReadDto.FromEntity(list));
    }

    [HttpGet("{projectId}/lists")]
    [ProducesResponseType(typeof(IEnumerable<ListReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetProjectLists(Guid projectId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null) return BadRequest("User ID cannot be null.");

        var project = await _context.Projects
            .Include(p => p.Members)
            .ThenInclude(pm => pm.WorkspaceMember)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound($"Project with ID {projectId} not found");

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         project.Members.Any(pm =>
                             pm.WorkspaceMember.UserId == userId
                         );

        if (!hasAccess)
            return Forbid("User does not have access to this project");

        var lists = await _context.Lists
            .Include(l => l.Tasks)
            .Where(l => l.ProjectId == projectId)
            .OrderBy(l => l.Position)
            .ToListAsync();

        return Ok(lists.Select(ListReadDto.FromEntity));
    }
}
