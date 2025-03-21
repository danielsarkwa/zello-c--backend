using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;
using Zello.Infrastructure.Data;

namespace Zello.Infrastructure.Repositories;

public class ProjectRepository : BaseRepository<Project>, IProjectRepository {
    public ProjectRepository(ApplicationDbContext context) : base(context) {
    }

    public async Task<Project?> GetProjectByIdWithDetailsAsync(Guid projectId) {
        return await _dbSet
            .Where(p => p.Id == projectId)
            .Select(p => new Project {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                EndDate = p.EndDate,
                StartDate = p.StartDate,
                Status = p.Status,
                WorkspaceId = p.WorkspaceId,
                Members = p.Members,
                CreatedDate = p.CreatedDate,
                Lists = p.Lists.Select(l => new TaskList {
                    Id = l.Id,
                    Name = l.Name,
                    Position = l.Position,
                    CreatedDate = l.CreatedDate,
                    ProjectId = l.ProjectId,
                    Tasks = l.Tasks.Select(t => new WorkTask {
                        Id = t.Id,
                        ProjectId = t.ProjectId,
                        ListId = t.ListId,
                        Name = t.Name,
                        Description = t.Description,
                        Priority = t.Priority,
                        Status = t.Status,
                        Assignees = t.Assignees,
                        CreatedDate = t.CreatedDate
                    }).ToList()
                }).ToList()
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<List<Project>> GetProjectsByWorkspaceAsync(Guid? workspaceId) {
        var query = _dbSet
            .Include(p => p.Lists)
            .ThenInclude(l => l.Tasks)
            .Include(p => p.Members)
            .AsQueryable();

        if (workspaceId.HasValue)
            query = query.Where(p => p.WorkspaceId == workspaceId.Value);

        return await query.ToListAsync();
    }

    public void Remove(Project project) {
        _dbSet.Remove(project);
    }

    public async Task<Project?> GetProjectWithMembersAsync(Guid projectId) {
        return await _dbSet
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);
    }

    public async Task AddProjectMemberAsync(ProjectMember projectMember) {
        // First check if the entity is already being tracked
        var existing = _context.ChangeTracker.Entries<WorkspaceMember>()
            .FirstOrDefault(e => e.Entity.Id == projectMember.WorkspaceMemberId);

        if (existing != null) {
            // Use the already tracked entity
            projectMember.WorkspaceMember = existing.Entity;
        } else {
            // Load the workspace member if not already tracked
            var workspaceMember = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm => wm.Id == projectMember.WorkspaceMemberId);

            if (workspaceMember == null) {
                throw new KeyNotFoundException(
                    $"WorkspaceMember with ID {projectMember.WorkspaceMemberId} not found");
            }

            projectMember.WorkspaceMember = workspaceMember;
        }

        await _context.ProjectMembers.AddAsync(projectMember);
    }

    public async Task<int> GetMaxListPositionAsync(Guid projectId) {
        return await _context.Lists
            .Where(t => t.ProjectId == projectId)
            .MaxAsync(t => (int?)t.Position) ?? 0;
    }

    public async Task AddListAsync(TaskList list) {
        await _context.Lists.AddAsync(list);
    }

    public async Task<bool> ExistsAsync(Guid projectId) {
        return await _context.Projects.AnyAsync(p => p.Id == projectId);
    }

    public async Task<List<TaskList>> GetProjectListsAsync(Guid projectId) {
        return await _context.Lists
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<ProjectMember?>
        GetProjectMemberAsync(Guid projectId, Guid workspaceMemberId) {
        return await _context.ProjectMembers
            .FirstOrDefaultAsync(pm =>
                pm.ProjectId == projectId &&
                pm.WorkspaceMemberId == workspaceMemberId);
    }
}
