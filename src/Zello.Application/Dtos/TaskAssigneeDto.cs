using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;

namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object for reading task assignee information
/// </summary>
/// <example>
/// {
///     "id": "123e4567-e89b-12d3-a456-426614174000",
///     "taskId": "123e4567-e89b-12d3-a456-426614174001",
///     "userId": "123e4567-e89b-12d3-a456-426614174002",
///     "assignedDate": "2024-01-01T12:00:00Z",
///     "user": { ... }
/// }
/// </example>
public class TaskAssigneeReadDto {
    /// <summary>
    /// Unique identifier of the task assignment
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the task
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    public Guid TaskId { get; set; }

    /// <summary>
    /// ID of the assigned user
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174002</example>
    public Guid UserId { get; set; }

    /// <summary>
    /// Date when the user was assigned to the task
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    public DateTime AssignedDate { get; set; }

    /// <summary>
    /// Detailed information about the assigned user
    /// </summary>
    public UserReadDto User { get; set; } = new UserReadDto();

    public static TaskAssigneeReadDto FromEntity(TaskAssignee taskAssignee) {
        return new TaskAssigneeReadDto {
            Id = taskAssignee.Id,
            TaskId = taskAssignee.TaskId,
            UserId = taskAssignee.UserId,
            AssignedDate = taskAssignee.AssignedDate,
            User = UserReadDto.FromEntity(taskAssignee.User)
        };
    }
}

/// <summary>
/// Data transfer object for creating a task assignment
/// </summary>
/// <example>
/// {
///     "userId": "123e4567-e89b-12d3-a456-426614174000"
/// }
/// </example>
public class TaskAssigneeCreateDto {
    /// <summary>
    /// ID of the user to assign to the task
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [Required]
    [JsonProperty("userId")]
    public Guid UserId { get; set; }

    public TaskAssignee ToEntity(Guid taskId) {
        return new TaskAssignee {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = UserId,
            AssignedDate = DateTime.UtcNow
        };
    }
}
