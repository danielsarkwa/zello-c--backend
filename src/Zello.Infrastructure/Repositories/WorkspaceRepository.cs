using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Repositories;

public class WorkspaceRepository : BaseRepository<Workspace>, IWorkspaceRepository {
    public WorkspaceRepository(ApplicationDbContext context) : base(context) {
    }

    public async Task<Workspace> GetWorkspaceWithDetailsAsync(Guid workspaceId) {
        var workspace = await _dbSet
            .Include(w => w.Members)
            .Include(w => w.Projects).ThenInclude(p => p.Lists).ThenInclude(l => l.Tasks)
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null) {
            throw new KeyNotFoundException($"Workspace with ID {workspaceId} was not found.");
        }

        return workspace;
    }

    public async Task<List<Workspace>> GetAllWorkspacesWithDetailsAsync() {
        return await _dbSet
            .Include(w => w.Members)
            .Include(w => w.Projects)
            .ThenInclude(p => p.Members)
            .ToListAsync();
    }

    public async Task<WorkspaceMember> AddWorkspaceMemberAsync(WorkspaceMember member) {
        await _context.WorkspaceMembers.AddAsync(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<List<WorkspaceMember>> GetWorkspaceMembersAsync(Guid workspaceId) {
        return await _context.WorkspaceMembers
            .Where(m => m.WorkspaceId == workspaceId)
            .ToListAsync();
    }

    public async Task<WorkspaceMember?> GetMemberByIdAsync(Guid memberId) {
        return await _context.WorkspaceMembers
            .Include(m => m.Workspace)
            .FirstOrDefaultAsync(m => m.Id == memberId);
    }

    public async Task<WorkspaceMember> UpdateMemberAsync(WorkspaceMember member) {
        _context.WorkspaceMembers.Update(member);
        await _context.SaveChangesAsync();
        return member;
    }
}
