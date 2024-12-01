using Zello.Domain.Entities;

namespace Zello.Domain.RepositoryInterfaces;

public interface ITaskAssigneeRepository : IBaseRepository<TaskAssignee> {
    Task<TaskAssignee> GetTaskAssigneeAsync(Guid taskId, Guid userId);
    Task<IEnumerable<TaskAssignee>> GetTaskAssigneesByTaskIdAsync(Guid taskId);
    Task<TaskAssignee> AddAssigneeAsync(TaskAssignee taskAssignee);
}
