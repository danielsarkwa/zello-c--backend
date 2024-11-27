using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Application.Dtos;
// using Zello.Application.Features.Projects;
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
    public async Task<IActionResult> CreateProject([FromBody] ProjectCreateDto projectDto) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try {
            Guid? userId = ClaimsHelper.GetUserId(User);
            if (userId == null) return BadRequest("User ID cannot be null.");

            // Verify workspace exists
            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(w => w.Id == projectDto.WorkspaceId);
            if (workspace == null)
                return BadRequest("Invalid workspace ID");

            // Verify user is a member of the workspace
            var workspaceMember = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(m =>
                    m.WorkspaceId == projectDto.WorkspaceId && m.UserId == userId.Value);
            if (workspaceMember == null)
                return BadRequest("User is not a member of the workspace");

            // Use the ToEntity method to create the project
            var project = projectDto.ToEntity();

            // Create project member entry for creator with Owner access
            var projectMember = new ProjectMember {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                WorkspaceMemberId = workspaceMember.Id,
                AccessLevel = AccessLevel.Owner,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Projects.AddAsync(project);
            await _context.ProjectMembers.AddAsync(projectMember);
            await _context.SaveChangesAsync();

            // Fetch the created project with its relationships
            var createdProject = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == project.Id);

            // Convert to DTO for response
            var projectReadDto = ProjectReadDto.FromEntity(createdProject!);

            return CreatedAtAction(nameof(GetProjectById), new { projectId = project.Id },
                projectReadDto);
        } catch (Exception e) {
            return StatusCode(500, "Internal server error: " + e.Message);
        }
    }

    [HttpGet("{projectId}")]
    [ProducesResponseType(typeof(ProjectReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectById(Guid projectId) {
        var project = await _context.Projects
            .Include(p => p.Members)
            .Include(p => p.Lists)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound($"Project with ID {projectId} not found");

        return Ok(project);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProjects([FromQuery] Guid? workspaceId = null) {
        var query = _context.Projects
            .Include(p => p.Lists)
            .ThenInclude(l => l.Tasks)
            .Include(p => p.Members)
            .AsQueryable();

        if (workspaceId.HasValue)
            query = query.Where(p => p.WorkspaceId == workspaceId.Value);

        var projects = await query.ToListAsync();
        return Ok(projects);
    }

    [HttpPut("{projectId}")]
    [ProducesResponseType(typeof(ProjectReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(Guid projectId,
        [FromBody] ProjectUpdateDto updatedProject) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try {
            var project = await _context.Projects
                .Include(p => p.Members)
                .Include(p => p.Lists)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                return NotFound($"Project with ID {projectId} not found");

            // Use the ToEntity method to update the project
            project = updatedProject.ToEntity(project);

            await _context.SaveChangesAsync();

            // Convert to DTO for response
            var projectReadDto = ProjectReadDto.FromEntity(project);

            return Ok(projectReadDto);
        } catch (Exception e) {
            return StatusCode(500, "Internal server error: " + e.Message);
        }
    }

    [HttpDelete("{projectId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid projectId) {
        var project = await _context.Projects
            .Include(p => p.Lists)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
            return NotFound($"Project with ID {projectId} not found");

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

        // Get current user ID
        Guid? currentUserId = ClaimsHelper.GetUserId(User);
        if (currentUserId == null)
            return BadRequest("User ID cannot be null.");

        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == createMember.ProjectId);

        if (project == null)
            return NotFound($"Project with ID {createMember.ProjectId} not found");

        // Get current user's workspace membership
        var currentUserWorkspaceMember = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == project.WorkspaceId &&
                                       wm.UserId == currentUserId.Value);

        if (currentUserWorkspaceMember == null)
            return BadRequest("Current user is not a member of the workspace");

        // Check if current user has sufficient privileges
        if (createMember.AccessLevel > currentUserWorkspaceMember.AccessLevel)
            return StatusCode(StatusCodes.Status403Forbidden,
                "Cannot assign access level higher than your own");

        // Validate target workspace member exists
        var targetWorkspaceMember = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(wm => wm.Id == createMember.WorkspaceMemberId);
        if (targetWorkspaceMember == null)
            return BadRequest("Invalid workspace member ID");

        // NEW CHECK: Ensure project access level doesn't exceed workspace access level
        if (createMember.AccessLevel > targetWorkspaceMember.AccessLevel)
            return StatusCode(StatusCodes.Status403Forbidden,
                "Cannot assign project access level higher than user's workspace access level");

        // Validate that the workspace member belongs to the same workspace as the project
        if (targetWorkspaceMember.WorkspaceId != project.WorkspaceId)
            return BadRequest("Workspace member does not belong to the project's workspace");

        // Check if member already exists in project
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
            new { projectId = createMember.ProjectId }, projectMember);
    }

    [HttpPost("{projectId}/lists")]
    [ProducesResponseType(typeof(ListReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateList(Guid projectId, [FromBody] ListCreateDto listDto) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
            return NotFound($"Project with ID {projectId} not found");

        // Get max position for new list
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

        return CreatedAtAction(nameof(GetProjectById), new { projectId }, list);
    }

    [HttpGet("{projectId}/lists")]
    public async Task<IActionResult> GetProjectLists(Guid projectId) {
        if (!await _context.Projects.AnyAsync(p => p.Id == projectId))
            return NotFound($"Project with ID {projectId} not found");

        var lists = await _context.Lists
            .Include(l => l.Tasks)
            .Where(l => l.ProjectId == projectId)
            .OrderBy(l => l.Position)
            .ToListAsync();

        return Ok(lists);
    }
}
