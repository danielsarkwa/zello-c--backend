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
public sealed class WorkspacesController : ControllerBase {
    private readonly ApplicationDbContext _context;

    public WorkspacesController(ApplicationDbContext context) {
        _context = context;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Workspace>> CreateWorkspace(
        [FromBody] WorkspaceCreateDto createWorkspace) {
        if (createWorkspace == null) return BadRequest();

        try {
            Guid? userId = ClaimsHelper.GetUserId(User);
            if (userId == null) {
                return BadRequest("User ID cannot be null.");
            }

            // Verify user exists
            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) {
                return BadRequest("User not found");
            }

            // Create workspace entity
            var workspace = createWorkspace.ToEntity(userId.Value);

            // Create and add the workspace owner as a member
            var ownerMember = new WorkspaceMember {
                Id = Guid.NewGuid(),
                WorkspaceId = workspace.Id,
                UserId = userId.Value,
                AccessLevel = AccessLevel.Owner,
                CreatedDate = DateTime.UtcNow
            };

            // Add owner as a member to workspace
            workspace.Members = new List<WorkspaceMember> { ownerMember };

            await _context.Workspaces.AddAsync(workspace);
            await _context.SaveChangesAsync();

            // Fetch fresh workspace to return, but without including the User navigation property
            var savedWorkspace = await _context.Workspaces
                .Include(w => w.Members)
                .Include(w => w.Projects)
                .Select(w => new {
                    w.Id,
                    w.Name,
                    w.OwnerId,
                    w.CreatedDate,
                    Projects = w.Projects.Select(p => new {
                        p.Id,
                        p.Name,
                        p.WorkspaceId,
                        p.Status,
                        p.CreatedDate
                    }).ToList(),
                    Members = w.Members.Select(m => new {
                        m.Id,
                        m.WorkspaceId,
                        m.UserId,
                        m.AccessLevel,
                        m.CreatedDate
                    }).ToList()
                })
                .FirstOrDefaultAsync(w => w.Id == workspace.Id);

            if (savedWorkspace == null) {
                return StatusCode(500, "Failed to retrieve created workspace");
            }

            return CreatedAtAction(
                nameof(GetWorkspace),
                new { workspaceId = workspace.Id },
                savedWorkspace);
        } catch (Exception e) {
            Console.WriteLine("Failed to create workspace: " + e.Message);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WorkspaceReadDto>>> GetAllWorkspaces() {
        var workspaces = await _context.Workspaces
            .Include(w => w.Members)
            .Include(w => w.Projects)
            .ThenInclude(p => p.Members)
            .Select(w => new {
                w.Id,
                w.Name,
                w.OwnerId,
                w.CreatedDate,
                Members = w.Members,
                Projects = w.Projects.Select(p => new {
                    p.Id,
                    p.WorkspaceId,
                    p.Name,
                    p.Description,
                    p.StartDate,
                    p.EndDate,
                    p.Status,
                    p.CreatedDate,
                    Members = p.Members.Select(pm => new {
                        pm.Id,
                        pm.ProjectId,
                        pm.WorkspaceMemberId,
                        pm.AccessLevel,
                        pm.CreatedDate,
                        WorkspaceMember =
                            w.Members.FirstOrDefault(wm => wm.Id == pm.WorkspaceMemberId)
                    }).ToList()
                }).ToList()
            })
            .ToListAsync();

        var workspaceDtos = workspaces.Select(w => new WorkspaceReadDto {
            Id = w.Id,
            Name = w.Name,
            OwnerId = w.OwnerId,
            CreatedDate = w.CreatedDate,
            Members = w.Members.Select(WorkspaceMemberReadDto.FromEntity),
            Projects = w.Projects.Select(p => new ProjectReadDto {
                Id = p.Id,
                WorkspaceId = p.WorkspaceId,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                Members = p.Members.Select(m => new ProjectMemberReadDto {
                    Id = m.Id,
                    ProjectId = m.ProjectId,
                    WorkspaceMemberId = m.WorkspaceMemberId,
                    AccessLevel = m.AccessLevel,
                    CreatedDate = m.CreatedDate,
                    WorkspaceMember = WorkspaceMemberReadDto.FromEntity(m.WorkspaceMember)
                })
            })
        }).ToList();

        return Ok(workspaceDtos);
    }

    [HttpGet("{workspaceId}")]
    [ProducesResponseType(typeof(Workspace), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkspaceReadDto>> GetWorkspace(Guid workspaceId) {
        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound();

        return Ok(workspace);
    }

    [HttpPut("{workspaceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkspaceReadDto>> UpdateWorkspace(Guid workspaceId,
        [FromBody] WorkspaceUpdateDto workspaceUpdateDto) {
        var existingWorkspace = await _context.Workspaces.FindAsync(workspaceId);
        if (existingWorkspace == null)
            return NotFound();

        var updatedWorkspace = workspaceUpdateDto.ToEntity(existingWorkspace);

        _context.Workspaces.Update(updatedWorkspace);
        await _context.SaveChangesAsync();

        return Ok(updatedWorkspace);
    }

    [HttpDelete("{workspaceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkspace(Guid workspaceId) {
        try {
            var workspace = await _context.Workspaces
                .Include(w => w.Members)
                .Include(w => w.Projects)
                .ThenInclude(p => p.Lists)
                .ThenInclude(l => l.Tasks)
                .FirstOrDefaultAsync(w => w.Id == workspaceId);

            if (workspace == null)
                return NotFound();

            _context.Workspaces.Remove(workspace);
            await _context.SaveChangesAsync();

            return NoContent();
        } catch (Exception ex) {
            return StatusCode(500, $"Failed to delete workspace: {ex.Message}");
        }
    }

    [HttpGet("{workspaceId}/projects")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ProjectReadDto>>>
        GetWorkspaceProjects(Guid workspaceId) {
        if (!await _context.Workspaces.AnyAsync(w => w.Id == workspaceId))
            return NotFound();

        var projects = await _context.Projects
            .Where(p => p.WorkspaceId == workspaceId)
            .ToListAsync();

        return Ok(projects);
    }

    [HttpGet("{workspaceId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<WorkspaceMemberReadDto>>> GetWorkspaceMembers(
        Guid workspaceId) {
        if (!await _context.Workspaces.AnyAsync(w => w.Id == workspaceId))
            return NotFound();

        var members = await _context.WorkspaceMembers
            .Where(m => m.WorkspaceId == workspaceId)
            .ToListAsync();

        return Ok(members);
    }

    [HttpPost("{workspaceId}/members")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkspaceMemberReadDto>> AddWorkspaceMember(Guid workspaceId,
        [FromBody] WorkspaceMemberCreateDto createMember) {
        if (!await _context.Workspaces.AnyAsync(w => w.Id == workspaceId))
            return NotFound("Workspace not found");

        if (!await _context.Users.AnyAsync(u => u.Id == createMember.UserId))
            return NotFound("User not found");

        var member = new WorkspaceMember {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            UserId = createMember.UserId,
            AccessLevel = AccessLevel.Member,
            CreatedDate = DateTime.UtcNow
        };

        await _context.WorkspaceMembers.AddAsync(member);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkspaceMembers), new { workspaceId }, member);
    }
}
