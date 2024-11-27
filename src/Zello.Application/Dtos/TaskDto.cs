using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Enums;

namespace Zello.Application.Dtos;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zello.Domain.Entities.Dto;
using Zello.Domain.Enums;

public class TaskReadDto {
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    [JsonProperty("list_id")]
    public Guid ListId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("priority")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Priority Priority { get; set; }

    [JsonProperty("status")]
    [JsonConverter(typeof(StringEnumConverter))]
    public CurrentTaskStatus Status { get; set; }

    [JsonProperty("deadline")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? Deadline { get; set; }

    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("assignees")]
    public IEnumerable<TaskAssigneeDto> Assignees { get; set; } = new List<TaskAssigneeDto>();

    [JsonProperty("comments")]
    public IEnumerable<CommentReadDto> Comments { get; set; } = new List<CommentReadDto>();

    [JsonProperty("list")]
    public ListReadDto? List { get; set; }

    [JsonProperty("project")]
    public ProjectReadDto? Project { get; set; }

    public static TaskReadDto FromEntity(WorkTask task) {
        return new TaskReadDto {
            Id = task.Id,
            ProjectId = task.ProjectId,
            ListId = task.ListId,
            Name = task.Name,
            Description = task.Description,
            Priority = task.Priority,
            Status = task.Status,
            Deadline = task.Deadline,
            CreatedDate = task.CreatedDate,
            Assignees = task.Assignees.Select(a => new TaskAssigneeDto {
                Id = a.Id,
                TaskId = a.TaskId,
                UserId = a.UserId,
                AssignedDate = a.AssignedDate
            }).ToList(),
            Comments = task.Comments.Select(c => new CommentReadDto {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                Content = c.Content,
                CreatedDate = c.CreatedDate,
            }).ToList(),
            List = task.List != null
                ? new ListReadDto {
                    Id = task.List.Id,
                    Name = task.List.Name,
                    ProjectId = task.List.ProjectId,
                    CreatedDate = task.List.CreatedDate
                }
                : null,
            Project = task.Project != null
                ? new ProjectReadDto {
                    Id = task.Project.Id,
                    Name = task.Project.Name,
                    WorkspaceId = task.Project.WorkspaceId,
                    CreatedDate = task.Project.CreatedDate
                }
                : null
        };
    }
}

public class TaskCreateDto {
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    [Required]
    [EnumDataType(typeof(CurrentTaskStatus))]
    [JsonProperty("status")]
    public CurrentTaskStatus Status { get; set; }

    [EnumDataType(typeof(Priority))]
    [JsonProperty("priority")]
    public Priority Priority { get; set; }

    [DataType(DataType.DateTime)]
    [JsonProperty("deadline")]
    public DateTime? Deadline { get; set; }

    [Required]
    [JsonProperty("projectId")]
    public Guid ProjectId { get; set; }

    [Required]
    [JsonProperty("listId")]
    public Guid ListId { get; set; }

    public WorkTask ToEntity() {
        return new WorkTask {
            Id = Guid.NewGuid(),
            Name = Name,
            Description = Description ?? string.Empty,
            Status = Status,
            Priority = Priority,
            Deadline = Deadline,
            ProjectId = ProjectId,
            ListId = ListId,
            CreatedDate = DateTime.UtcNow,
        };
    }
}

public class TaskUpdateDto {
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("name")]
    public string? Name { get; set; }

    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    [EnumDataType(typeof(CurrentTaskStatus))]
    [JsonProperty("status")]
    public CurrentTaskStatus Status { get; set; }

    [EnumDataType(typeof(Priority))]
    [JsonProperty("priority")]
    public Priority Priority { get; set; }

    [DataType(DataType.DateTime)]
    [JsonProperty("deadline")]
    public DateTime Deadline { get; set; }

    public void UpdateEntity(WorkTask entity) {
        entity.Name = Name ?? entity.Name;
        entity.Description = Description ?? entity.Description;
        entity.Status = Status;
        entity.Priority = Priority;
        entity.Deadline = Deadline;
    }
}
