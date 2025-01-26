using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;
using Zello.Infrastructure.Data;
using Zello.Domain.RepositoryInterfaces;

namespace Zello.Infrastructure.Repositories;

public class TaskListRepository : BaseRepository<TaskList>, ITaskListRepository {
    public TaskListRepository(ApplicationDbContext context) : base(context) { }

    public async Task<TaskList?> GetByIdWithRelationsAsync(Guid id) {
        var taskList = await _dbSet
            .Include(l => l.Project)
            .ThenInclude(p => p.Workspace)
            .ThenInclude(w => w.Members)
            .Include(l => l.Tasks)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (taskList == null) {
            throw new InvalidOperationException($"TaskList with id {id} not found.");
        }

        Console.WriteLine($"TaskList retrieved: {taskList.Name}");
        foreach (var task in taskList.Tasks)
        {
            Console.WriteLine($"Task: {task.Name}");
        }

        return taskList;
    }

    public async Task<IEnumerable<TaskList>> GetAllWithRelationsAsync(Guid? projectId) {
        return await _dbSet
            .Where(l => !projectId.HasValue || l.ProjectId == projectId.Value)
            .Include(l => l.Tasks)
            .ThenInclude(t => t.Assignees)
            .Include(l => l.Tasks)
            .ThenInclude(t => t.Comments)
            .OrderBy(l => l.Position)
            .ToListAsync();
    }

    public async Task<TaskList> UpdatePositionAsync(Guid id, int newPosition) {
        var list = await GetByIdAsync(id);
        if (list == null) throw new InvalidOperationException($"TaskList with id {id} not found.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try {
            var projectLists = await _dbSet
                .Where(l => l.ProjectId == list.ProjectId)
                .OrderBy(l => l.Position)
                .ToListAsync();

            var oldPosition = list.Position;
            if (newPosition < oldPosition) {
                var listsToUpdate = await _dbSet
                    .Where(l => l.ProjectId == list.ProjectId &&
                               l.Position >= newPosition &&
                               l.Position < oldPosition)
                    .ToListAsync();

                foreach (var l in listsToUpdate) {
                    l.Position++;
                }
            } else if (newPosition > oldPosition) {
                var listsToUpdate = await _dbSet
                    .Where(l => l.ProjectId == list.ProjectId &&
                               l.Position > oldPosition &&
                               l.Position <= newPosition)
                    .ToListAsync();

                foreach (var l in listsToUpdate) {
                    l.Position--;
                }
            }

            list.Position = newPosition;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return list;
        } catch {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id) {
        return await _dbSet.AnyAsync(l => l.Id == id);
    }

    public async Task AddTaskAsync(WorkTask task) {
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<WorkTask>?> GetListTasksAsync(Guid listId) {
        return await _context.Tasks
            .Where(t => t.ListId == listId)
            .Include(t => t.Assignees)
            .Include(t => t.Comments)
            .OrderBy(t => t.CreatedDate)
            .ToListAsync();
    }
}
