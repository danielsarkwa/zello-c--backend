using Zello.Domain.Entities;

namespace Zello.Infrastructure.Interfaces;

public interface ITaskRepository : IRepository<WorkTask> {
    Task<IEnumerable<WorkTask>> GetTasksByProjectIdAsync(Guid projectId);
    Task<IEnumerable<WorkTask>> GetTasksByAssigneeIdAsync(Guid userId);
    Task<IEnumerable<WorkTask>> GetTasksByListIdAsync(Guid listId);
}
