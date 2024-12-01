using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;
using Zello.Infrastructure.Data;

namespace Zello.Infrastructure.Repositories;

public class TaskAssigneeRepository : BaseRepository<TaskAssignee>, ITaskAssigneeRepository {
    public TaskAssigneeRepository(ApplicationDbContext context) : base(context) {
    }

    public async Task<TaskAssignee> GetTaskAssigneeAsync(Guid taskId, Guid userId) {
        var taskAssignee = await _dbSet
            .FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.UserId == userId);
        if (taskAssignee == null) {
            throw new InvalidOperationException("TaskAssignee not found.");
        }
        return taskAssignee;
    }

    public async Task<IEnumerable<TaskAssignee>> GetTaskAssigneesByTaskIdAsync(Guid taskId) {
        return await _dbSet
            .Where(ta => ta.TaskId == taskId)
            .Include(ta => ta.User)
            .ToListAsync();
    }

    public async Task<TaskAssignee> AddAssigneeAsync(TaskAssignee taskAssignee) {
        await _dbSet.AddAsync(taskAssignee);
        await _context.SaveChangesAsync();

        var createdAssignee = await _dbSet
            .Include(ta => ta.Task)
            .Include(ta => ta.User)
            .FirstAsync(ta => ta.Id == taskAssignee.Id);

        return createdAssignee;
    }
}
