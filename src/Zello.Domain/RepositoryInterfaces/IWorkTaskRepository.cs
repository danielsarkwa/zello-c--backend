using Zello.Domain.Entities;

namespace Zello.Domain.RepositoryInterfaces;

public interface IWorkTaskRepository : IBaseRepository<WorkTask> {
    Task<WorkTask> GetTaskByIdAsync(Guid taskId);
    Task<IEnumerable<WorkTask>> GetAllTasksAsync();
    Task AddTaskAsync(WorkTask task);
    Task UpdateTaskAsync(WorkTask task);
    Task DeleteTaskAsync(Guid taskId);
    Task MoveTaskAsync(WorkTask task, Guid targetListId);
}

