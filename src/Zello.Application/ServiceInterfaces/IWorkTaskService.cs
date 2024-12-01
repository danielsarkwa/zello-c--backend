using Zello.Application.Dtos;
using Zello.Domain.Entities.Api.User;

public interface IWorkTaskService {
    Task<TaskReadDto> GetTaskByIdAsync(Guid taskId, Guid userId, AccessLevel userAccess);
    Task<IEnumerable<TaskReadDto>> GetAllTasksAsync(Guid userId, AccessLevel userAccess);
    Task<TaskReadDto> UpdateTaskAsync(Guid taskId, TaskUpdateDto updateDto, Guid userId, AccessLevel userAccess);
    Task DeleteTaskAsync(Guid taskId, Guid userId, AccessLevel userAccess);
    Task<TaskReadDto> MoveTaskAsync(Guid taskId, Guid targetListId, Guid userId, AccessLevel userAccess);
    Task<TaskAssigneeReadDto> AssignUserToTaskAsync(Guid taskId, Guid userToAssignId, Guid userId, AccessLevel userAccess);
    Task RemoveTaskAssigneeAsync(Guid taskId, Guid userToRemoveId, Guid userId, AccessLevel userAccess);
    Task<IEnumerable<TaskAssigneeReadDto>> GetTaskAssigneesAsync(Guid taskId, Guid userId, AccessLevel userAccess);
    Task<IEnumerable<CommentReadDto>> GetTaskCommentsAsync(Guid taskId, Guid userId, AccessLevel userAccess);
    Task<CommentReadDto> AddTaskCommentAsync(Guid taskId, string content, Guid userId, AccessLevel userAccess);
}
