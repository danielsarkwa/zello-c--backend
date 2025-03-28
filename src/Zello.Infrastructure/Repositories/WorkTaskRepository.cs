using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Repositories;

public class WorkTaskRepository : BaseRepository<WorkTask>, IWorkTaskRepository {

    public WorkTaskRepository(ApplicationDbContext context) : base(context) { }

    public async Task<WorkTask> GetTaskByIdAsync(Guid taskId) {
        var task = await _dbSet
            .Include(t => t.Assignees)
            .ThenInclude(a => a.User)
            .Include(t => t.Comments)
            .Include(t => t.Project)
            .Select(t => new WorkTask {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Deadline = t.Deadline,
                Priority = t.Priority,
                Status = t.Status,
                Assignees = t.Assignees.Select(a => new TaskAssignee {
                    Id = a.Id,
                    UserId = a.UserId,
                    TaskId = a.TaskId,
                    AssignedDate = a.AssignedDate
                }).ToList(),
                Comments = t.Comments.Select(c => new Comment {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedDate = c.CreatedDate,
                    UserId = c.UserId,
                }).ToList(),
                ListId = t.ListId,
                ProjectId = t.ProjectId,
                Project = new Project {
                    Id = t.Project.Id,
                    Name = t.Project.Name,
                    WorkspaceId = t.Project.WorkspaceId,
                    Status = t.Project.Status,
                    CreatedDate = t.Project.CreatedDate
                }
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null) {
            throw new KeyNotFoundException($"Task with ID {taskId} was not found.");
        }

        return task;
    }

    public async Task<IEnumerable<WorkTask>> GetAllTasksAsync() {
        return await _dbSet
            .Include(t => t.Assignees)
            .ThenInclude(a => a.User)
            .Include(t => t.Comments)
            .Include(t => t.List)
            .Include(t => t.Project)
            .ToListAsync();
    }

    public async Task AddTaskAsync(WorkTask task) {
        await _dbSet.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTaskAsync(WorkTask task) {
        _dbSet.Update(task);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(Guid taskId) {
        var task = await _dbSet.FindAsync(taskId);
        if (task != null) {
            _dbSet.Remove(task);
            await _context.SaveChangesAsync();
        }
    }

    public async Task MoveTaskAsync(WorkTask task, Guid targetListId) {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try {
            task.ListId = targetListId;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        } catch (Exception) {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
