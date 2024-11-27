using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;

namespace Zello.Application.Dtos;

public class TaskAssigneeReadDto {
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AssignedDate { get; set; }

    // Only keep the User related entity
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


public class TaskAssigneeCreateDto {
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
