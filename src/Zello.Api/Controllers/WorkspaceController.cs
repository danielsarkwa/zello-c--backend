using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Helpers;

/// <summary>
/// Controller for managing workspaces and their associated resources including members and projects
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class WorkspacesController : ControllerBase {
    private readonly ApplicationDbContext _context;

    public WorkspacesController(ApplicationDbContext context) {
        _context = context;
    }

    private async Task<bool> CanManageWorkspace(Guid workspaceId, Guid userId,
        AccessLevel? userAccess) {
        if (userAccess == AccessLevel.Admin)
            return true;

        var memberAccess = await _context.WorkspaceMembers
            .Where(m => m.WorkspaceId == workspaceId && m.UserId == userId)
            .Select(m => m.AccessLevel)
            .FirstOrDefaultAsync();

        return memberAccess >= AccessLevel.Owner;
    }

    /// <summary>
    /// Deletes a workspace
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace to delete</param>
    /// <remarks>
    /// Only workspace owners and administrators can delete workspaces.
    /// Deleting a workspace will also delete all associated projects, lists, and tasks.
    /// </remarks>
    /// <response code="204">Workspace successfully deleted</response>
    /// <response code="404">Workspace not found</response>
    /// <response code="403">User does not have permission to delete the workspace</response>
    [HttpDelete("{workspaceId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteWorkspace(Guid workspaceId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound();

        if (!await CanManageWorkspace(workspaceId, userId.Value, userAccess))
            return Forbid("Insufficient permissions to delete workspace");

        _context.Workspaces.Remove(workspace);
        await _context.SaveChangesAsync();

        return NoContent();
    }


    // In WorkspacesController.cs
    /// <summary>
    /// Updates the access level of a workspace member
    /// </summary>
    /// <param name="elevation">The member elevation details including member ID and new access level</param>
    /// <remarks>
    /// Requires the following permissions:
    /// - Admin access, or
    /// - Workspace Owner access level
    ///
    /// Access level restrictions:
    /// - Cannot assign higher access than your own level (except admins)
    /// - Cannot modify access if you're below Owner level
    /// - Cannot assign Admin level unless you are an Admin
    ///
    /// All project access levels for this member will be validated to ensure they don't
    /// exceed the new workspace access level.
    /// </remarks>
    /// <response code="200">Member access level successfully updated</response>
    /// <response code="400">Invalid elevation data provided</response>
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
        if (userId == null)
            return BadRequest("User ID missing");

        var member = await _context.WorkspaceMembers
            .Include(wm => wm.Workspace)
            .ThenInclude(w => w.Members)
            .FirstOrDefaultAsync(m => m.Id == elevation.MemberId);

        if (member == null)
            return NotFound("Member not found");

        if (!await CanManageWorkspace(member.WorkspaceId, userId.Value, userAccess))
            return Forbid("Insufficient permissions to manage workspace members");

        var currentUserAccess = member.Workspace.Members
            .FirstOrDefault(m => m.UserId == userId.Value)?.AccessLevel ?? AccessLevel.Member;

        if (elevation.NewAccessLevel > currentUserAccess && userAccess != AccessLevel.Admin)
            return Forbid("Cannot assign access level higher than your own");

        member.AccessLevel = elevation.NewAccessLevel;
        await _context.SaveChangesAsync();

        return Ok(WorkspaceMemberReadDto.FromEntity(member));
    }

    /// <summary>
    /// Retrieves all workspaces accessible to the current user
    /// </summary>
    /// <remarks>
    /// For regular users, returns only workspaces where they are members.
    /// For administrators, returns all workspaces in the system.
    /// The response includes details about workspace members, projects, and associated lists.
    /// </remarks>
    /// <response code="200">List of accessible workspaces</response>
    /// <response code="403">User is not authorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<WorkspaceReadDto>>> GetAllWorkspaces() {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var query = _context.Workspaces
            .Include(w => w.Members)
            .Include(w => w.Projects)
            .ThenInclude(p => p.Lists)
            .Include(w => w.Projects)
            .ThenInclude(p => p.Members)
            .AsQueryable();

        if (userAccess != AccessLevel.Admin) {
            query = query.Where(w =>
                w.Members.Any(m => m.UserId == userId)
            );
        }

        var workspaces = await query
            .Select(w => new WorkspaceReadDto {
                Id = w.Id,
                Name = w.Name,
                OwnerId = w.OwnerId,
                CreatedDate = w.CreatedDate,
                Members = w.Members.Select(m => new WorkspaceMemberReadDto {
                    Id = m.Id,
                    WorkspaceId = m.WorkspaceId,
                    UserId = m.UserId,
                    AccessLevel = m.AccessLevel,
                    CreatedDate = m.CreatedDate
                }),
                Projects = w.Projects.Select(p => new ProjectReadDto {
                    Id = p.Id,
                    WorkspaceId = p.WorkspaceId,
                    Name = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Status = p.Status,
                    CreatedDate = p.CreatedDate,
                    Lists = p.Lists.Select(l => new ListReadDto {
                        Id = l.Id,
                        ProjectId = l.ProjectId,
                        Name = l.Name,
                        Position = l.Position,
                        CreatedDate = l.CreatedDate
                    })
                })
            })
            .ToListAsync();

        return Ok(workspaces);
    }

    /// <summary>
    /// Retrieves a specific workspace by its ID
    /// </summary>
    /// <param name="workspaceId">The unique identifier of the workspace to retrieve</param>
    /// <remarks>
    /// Returns detailed information about the workspace including:
    /// - Basic workspace information (name, owner, creation date)
    /// - List of workspace members and their access levels
    /// - Associated projects and their task lists
    /// </remarks>
    /// <response code="200">The requested workspace details</response>
    /// <response code="404">Workspace not found</response>
    /// <response code="403">User does not have access to this workspace</response>
    [HttpGet("{workspaceId}")]
    [ProducesResponseType(typeof(WorkspaceReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceReadDto>> GetWorkspace(Guid workspaceId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .Include(w => w.Projects)
            .ThenInclude(p => p.Lists)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound();

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         workspace.Members.Any(m => m.UserId == userId);

        if (!hasAccess)
            return Forbid("User does not have access to this workspace");

        var workspaceDto = new WorkspaceReadDto {
            Id = workspace.Id,
            Name = workspace.Name,
            OwnerId = workspace.OwnerId,
            CreatedDate = workspace.CreatedDate,
            Members = workspace.Members.Select(m => new WorkspaceMemberReadDto {
                Id = m.Id,
                WorkspaceId = m.WorkspaceId,
                UserId = m.UserId,
                AccessLevel = m.AccessLevel,
                CreatedDate = m.CreatedDate
            }),
            Projects = workspace.Projects.Select(p => new ProjectReadDto {
                Id = p.Id,
                WorkspaceId = p.WorkspaceId,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                Lists = p.Lists.Select(l => new ListReadDto {
                    Id = l.Id,
                    ProjectId = l.ProjectId,
                    Name = l.Name,
                    Position = l.Position,
                    CreatedDate = l.CreatedDate
                })
            })
        };

        return Ok(workspaceDto);
    }

    /// <summary>
    /// Creates a new workspace
    /// </summary>
    /// <param name="createWorkspace">The workspace creation details</param>
    /// <remarks>
    /// Creates a new workspace and automatically:
    /// - Assigns the current user as the workspace owner
    /// - Adds the creator as a workspace member with Owner access level
    /// - Initializes an empty project list
    /// </remarks>
    /// <response code="201">Workspace created successfully</response>
    /// <response code="400">Invalid workspace data provided</response>
    [HttpPost]
    [ProducesResponseType(typeof(WorkspaceReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Workspace>> CreateWorkspace(
        [FromBody] WorkspaceCreateDto createWorkspace) {
        if (createWorkspace == null) return BadRequest();

        try {
            var userId = ClaimsHelper.GetUserId(User);
            if (userId == null)
                return BadRequest("User ID cannot be null.");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return BadRequest("User not found");

            var workspace = createWorkspace.ToEntity(userId.Value);

            var ownerMember = new WorkspaceMember {
                Id = Guid.NewGuid(),
                WorkspaceId = workspace.Id,
                UserId = userId.Value,
                AccessLevel = AccessLevel.Owner,
                CreatedDate = DateTime.UtcNow
            };

            workspace.Members = new List<WorkspaceMember> { ownerMember };

            await _context.Workspaces.AddAsync(workspace);
            await _context.SaveChangesAsync();

            var savedWorkspace = await _context.Workspaces
                .Include(w => w.Members)
                .Include(w => w.Projects)
                .Where(w => w.Id == workspace.Id)
                .Select(w => new WorkspaceReadDto {
                    Id = w.Id,
                    Name = w.Name,
                    OwnerId = w.OwnerId,
                    CreatedDate = w.CreatedDate,
                    Projects = w.Projects.Select(p => new ProjectReadDto {
                        Id = p.Id,
                        Name = p.Name,
                        WorkspaceId = p.WorkspaceId,
                        Status = p.Status,
                        CreatedDate = p.CreatedDate,
                        Lists = p.Lists.Select(l => new ListReadDto {
                            Id = l.Id,
                            ProjectId = l.ProjectId,
                            Name = l.Name,
                            Position = l.Position,
                            CreatedDate = l.CreatedDate
                        })
                    }),
                    Members = w.Members.Select(m => new WorkspaceMemberReadDto {
                        Id = m.Id,
                        WorkspaceId = m.WorkspaceId,
                        UserId = m.UserId,
                        AccessLevel = m.AccessLevel,
                        CreatedDate = m.CreatedDate
                    })
                })
                .FirstOrDefaultAsync();

            if (savedWorkspace == null)
                return StatusCode(500, "Failed to retrieve created workspace");

            return CreatedAtAction(nameof(GetWorkspace), new { workspaceId = workspace.Id },
                savedWorkspace);
        } catch (Exception e) {
            return StatusCode(500, "Internal server error." + e.Message);
        }
    }

    /// <summary>
    /// Updates an existing workspace
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace to update</param>
    /// <param name="workspaceUpdateDto">The updated workspace details</param>
    /// <remarks>
    /// Only workspace owners and administrators can update workspace details.
    /// Currently supports updating the workspace name only.
    /// </remarks>
    /// <response code="200">Workspace updated successfully</response>
    /// <response code="404">Workspace not found</response>
    /// <response code="403">User does not have permission to update the workspace</response>
    [HttpPut("{workspaceId}")]
    [ProducesResponseType(typeof(WorkspaceReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceReadDto>> UpdateWorkspace(
        Guid workspaceId,
        [FromBody] WorkspaceUpdateDto workspaceUpdateDto) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound();

        if (!await CanManageWorkspace(workspaceId, userId.Value, userAccess))
            return Forbid("Insufficient permissions to update workspace");

        var updatedWorkspace = workspaceUpdateDto.ToEntity(workspace);
        _context.Workspaces.Update(updatedWorkspace);
        await _context.SaveChangesAsync();

        var workspaceDto = new WorkspaceReadDto {
            Id = updatedWorkspace.Id,
            Name = updatedWorkspace.Name,
            OwnerId = updatedWorkspace.OwnerId,
            CreatedDate = updatedWorkspace.CreatedDate,
            Members = updatedWorkspace.Members.Select(m => new WorkspaceMemberReadDto {
                Id = m.Id,
                WorkspaceId = m.WorkspaceId,
                UserId = m.UserId,
                AccessLevel = m.AccessLevel,
                CreatedDate = m.CreatedDate
            })
        };

        return Ok(workspaceDto);
    }

    /// <summary>
    /// Retrieves all members of a specific workspace
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace</param>
    /// <remarks>
    /// Returns a list of all members in the workspace along with their access levels.
    /// Only accessible to workspace members and administrators.
    /// </remarks>
    /// <response code="200">List of workspace members</response>
    /// <response code="404">Workspace not found</response>
    /// <response code="403">User does not have access to this workspace</response>
    [HttpGet("{workspaceId}/members")]
    [ProducesResponseType(typeof(IEnumerable<WorkspaceMemberReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<WorkspaceMemberReadDto>>> GetWorkspaceMembers(
        Guid workspaceId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound();

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         workspace.Members.Any(m => m.UserId == userId);

        if (!hasAccess)
            return Forbid("User does not have access to this workspace");

        var members = workspace.Members
            .Select(m => new WorkspaceMemberReadDto {
                Id = m.Id,
                WorkspaceId = m.WorkspaceId,
                UserId = m.UserId,
                AccessLevel = m.AccessLevel,
                CreatedDate = m.CreatedDate
            })
            .ToList();

        return Ok(members);
    }

    /// <summary>
    /// Adds a new member to a workspace
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace</param>
    /// <param name="createMember">The member details to add</param>
    /// <remarks>
    /// Only workspace owners and administrators can add new members.
    /// The access level assigned cannot be higher than the user performing the action.
    /// </remarks>
    /// <response code="201">Member added successfully</response>
    /// <response code="404">Workspace or user not found</response>
    /// <response code="400">User is already a member</response>
    /// <response code="403">Insufficient permissions to add members</response>
    [HttpPost("{workspaceId}/members")]
    [ProducesResponseType(typeof(WorkspaceMemberReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<WorkspaceMemberReadDto>> AddWorkspaceMember(
        Guid workspaceId,
        [FromBody] WorkspaceMemberCreateDto createMember) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound("Workspace not found");

        if (!await CanManageWorkspace(workspaceId, userId.Value, userAccess))
            return Forbid("Insufficient permissions to manage workspace members");

        if (!await _context.Users.AnyAsync(u => u.Id == createMember.UserId))
            return NotFound("User not found");

        if (workspace.Members.Any(m => m.UserId == createMember.UserId))
            return BadRequest("User is already a member of this workspace");

        var currentUserAccess = workspace.Members
            .FirstOrDefault(m => m.UserId == userId.Value)?.AccessLevel ?? AccessLevel.Member;

        if (createMember.AccessLevel > currentUserAccess && userAccess != AccessLevel.Admin)
            return Forbid("Cannot assign access level higher than your own");

        var member = createMember.ToEntity(workspaceId);
        member.Id = Guid.NewGuid();
        member.CreatedDate = DateTime.UtcNow;

        await _context.WorkspaceMembers.AddAsync(member);
        await _context.SaveChangesAsync();

        var memberDto = new WorkspaceMemberReadDto {
            Id = member.Id,
            WorkspaceId = member.WorkspaceId,
            UserId = member.UserId,
            AccessLevel = member.AccessLevel,
            CreatedDate = member.CreatedDate
        };

        return CreatedAtAction(nameof(GetWorkspaceMembers), new { workspaceId }, memberDto);
    }

    /// <summary>
    /// Retrieves all projects in a workspace
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace</param>
    /// <remarks>
    /// Returns a list of all projects in the workspace.
    /// Only accessible to workspace members and administrators.
    /// </remarks>
    /// <response code="200">List of workspace projects</response>
    /// <response code="404">Workspace not found</response>
    /// <response code="403">User does not have access to this workspace</response>
    [HttpGet("{workspaceId}/projects")]
    [ProducesResponseType(typeof(IEnumerable<ProjectReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ProjectReadDto>>> GetWorkspaceProjects(
        Guid workspaceId) {
        var userId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        if (userId == null)
            return BadRequest("User ID missing");

        var workspace = await _context.Workspaces
            .Include(w => w.Members)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
            return NotFound();

        bool hasAccess = userAccess == AccessLevel.Admin ||
                         workspace.Members.Any(m => m.UserId == userId);

        if (!hasAccess)
            return Forbid("User does not have access to this workspace");

        var projects = await _context.Projects
            .Where(p => p.WorkspaceId == workspaceId)
            .Select(p => new ProjectReadDto {
                Id = p.Id,
                WorkspaceId = p.WorkspaceId,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                CreatedDate = p.CreatedDate
            })
            .ToListAsync();

        return Ok(projects);
    }
}
