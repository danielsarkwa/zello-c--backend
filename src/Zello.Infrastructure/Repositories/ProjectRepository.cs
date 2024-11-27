using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Interfaces;

namespace Zello.Infrastructure.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository {
    public ProjectRepository(ApplicationDbContext context) : base(context) {
    }

    public async Task<IEnumerable<Project>> GetProjectsByWorkspaceIdAsync(Guid workspaceId) {
        return await _context.Projects
            .Include(p => p.Members)
            .Where(p => p.WorkspaceId == workspaceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(Guid userId) {
        return await _context.Projects
            .Include(p => p.Members)
            .Where(p => p.Members.Any(m => m.WorkspaceMember.UserId == userId))
            .ToListAsync();
    }
}
