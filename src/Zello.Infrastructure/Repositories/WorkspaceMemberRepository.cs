using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;
using Microsoft.EntityFrameworkCore;
using Zello.Infrastructure.Data;

namespace Zello.Infrastructure.Repositories;

public class WorkspaceMemberRepository : BaseRepository<WorkspaceMember>, IWorkspaceMemberRepository {
    public WorkspaceMemberRepository(ApplicationDbContext context) : base(context) {
    }

    public async Task<WorkspaceMember?> GetWorkspaceMemberAsync(Guid workspaceId, Guid userId) {
        return await _dbSet
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);
    }

    public async Task<IEnumerable<WorkspaceMember>> GetMembersByWorkspaceIdAsync(Guid workspaceId) {
        return await _context.WorkspaceMembers
            .Where(m => m.WorkspaceId == workspaceId)
            .ToListAsync();
    }
}
