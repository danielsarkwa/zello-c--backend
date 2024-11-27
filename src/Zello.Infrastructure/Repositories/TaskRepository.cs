using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Interfaces;

namespace Zello.Infrastructure.Repositories;

public class TaskRepository : Repository<WorkTask>, ITaskRepository {
    public TaskRepository(ApplicationDbContext context) : base(context) {
    }

    public async Task<IEnumerable<WorkTask>> GetTasksByProjectIdAsync(Guid projectId) {
        return await _context.Tasks
            .Include(t => t.Assignees)
            .Include(t => t.Comments)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkTask>> GetTasksByAssigneeIdAsync(Guid userId) {
        return await _context.Tasks
            .Include(t => t.Assignees)
            .Include(t => t.Comments)
            .Where(t => t.Assignees.Any(a => a.UserId == userId))
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkTask>> GetTasksByListIdAsync(Guid listId) {
        return await _context.Tasks
            .Include(t => t.Assignees)
            .Include(t => t.Comments)
            .Where(t => t.ListId == listId)
            .ToListAsync();
    }
}
