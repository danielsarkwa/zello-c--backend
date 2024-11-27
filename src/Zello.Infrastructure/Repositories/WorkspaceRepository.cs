using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Interfaces;

namespace Zello.Infrastructure.Repositories;

public class WorkspaceRepository : Repository<Workspace>, IWorkspaceRepository {
    public WorkspaceRepository(ApplicationDbContext context) : base(context) {
    }

    public async Task<IEnumerable<Workspace>> GetWorkspacesByUserIdAsync(Guid userId) {
        return await _context.Workspaces
            .Include(w => w.Members)
            .Where(w => w.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task<bool> IsUserWorkspaceOwnerAsync(Guid userId, Guid workspaceId) {
        return await _context.Workspaces
            .AnyAsync(w => w.Id == workspaceId && w.OwnerId == userId);
    }
}
