using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Zello.Application.ServiceImplementations {

  public class AuthorizationService : IAuthorizationService {
    private readonly ApplicationDbContext _context;

    public AuthorizationService(ApplicationDbContext context) {
      _context = context;
    }

    public async Task<bool> AuthorizeProjectAccessAsync(
        Guid userId,
        Guid projectId,
        AccessLevel requiredAccessLevel
    ) {
      var project = await _context.Projects
          .Include(p => p.Members)
          .ThenInclude(pm => pm.WorkspaceMember)
          .FirstOrDefaultAsync(p => p.Id == projectId);

      if (project == null) return false;

      var member = project.Members.FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);
      if (member == null) return false;

      return member.AccessLevel >= requiredAccessLevel;
    }

    public async Task<bool> AuthorizeProjectMembershipAsync(
            Guid userId,
            Guid projectId
        ) {
      var project = await _context.Projects
          .Include(p => p.Members)
          .ThenInclude(pm => pm.WorkspaceMember)
          .FirstOrDefaultAsync(p => p.Id == projectId);

      if (project == null) return false;

      return project.Members.Any(pm => pm.WorkspaceMember.UserId == userId);
    }

    public async Task<bool> AuthorizeWorkspaceMembershipAsync(Guid workspaceId, Guid userId) {
      var workspace = await _context.Workspaces
          .Include(w => w.Members)
          .FirstOrDefaultAsync(w => w.Id == workspaceId);

      if (workspace == null)
        return false;

      var workspaceMember = workspace.Members
          .FirstOrDefault(m => m.UserId == userId);

      return workspaceMember != null;
    }

    public async Task<bool> HasSufficientMembershipPermissionsAsync(Guid workspaceId, Guid userId, AccessLevel userAccess) {
      var workspace = await _context.Workspaces
      .Include(w => w.Members)
      .FirstOrDefaultAsync(w => w.Id == workspaceId);

      if (workspace == null) return false;

      var workspaceMember = workspace.Members
        .FirstOrDefault(m => m.UserId == userId);

      if (workspaceMember == null) return false;

      return workspaceMember.AccessLevel >= AccessLevel.Member || userAccess == AccessLevel.Admin;
    }

    public async Task<bool> CanManageMembersAsync(Guid userId, Guid projectId, AccessLevel userAccess) {
      if (userAccess == AccessLevel.Admin) return true;

      var project = await _context.Projects
          .Include(p => p.Members)
          .ThenInclude(pm => pm.WorkspaceMember)
          .FirstOrDefaultAsync(p => p.Id == projectId);

      if (project == null) return false;

      var currentProjectMember = project.Members
          .FirstOrDefault(pm => pm.WorkspaceMember.UserId == userId);

      return currentProjectMember?.AccessLevel >= AccessLevel.Owner;
    }

    public async Task<bool> AuthorizeCommentAccessAsync(
        Guid userId,
        Guid commentId,
        AccessLevel? currentUserAccessLevel
    ) {
      if (currentUserAccessLevel == AccessLevel.Admin) return true;

      var comment = await _context.Comments
          .Include(c => c.Task)
          .ThenInclude(t => t.List)
          .ThenInclude(l => l.Project)
          .FirstOrDefaultAsync(c => c.Id == commentId);

      if (comment == null) return false;

      return await AuthorizeProjectMembershipAsync(userId, comment.Task.List.Project.Id);
    }
  }
}