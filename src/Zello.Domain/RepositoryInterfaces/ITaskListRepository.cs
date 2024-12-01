using Zello.Domain.Entities;

namespace Zello.Domain.RepositoryInterfaces;

public interface ITaskListRepository : IBaseRepository<TaskList> {
    Task<TaskList?> GetByIdWithRelationsAsync(Guid id);
    Task<IEnumerable<TaskList>> GetAllWithRelationsAsync(Guid? projectId);
    Task<TaskList> UpdatePositionAsync(Guid id, int newPosition);
    Task<bool> ExistsAsync(Guid id);
    Task AddTaskAsync(WorkTask task);
    Task<IEnumerable<WorkTask>?> GetListTasksAsync(Guid listId);
}
