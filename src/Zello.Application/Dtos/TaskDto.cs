using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Enums;
using Newtonsoft.Json.Converters;
using Zello.Domain.Entities.Dto;

namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object for reading task information
/// </summary>
public class TaskReadDto {
    /// <summary>
    /// Unique identifier for the task
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the project this task belongs to
    /// </summary>
    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// ID of the list this task belongs to
    /// </summary>
    [JsonProperty("list_id")]
    public Guid ListId { get; set; }

    /// <summary>
    /// Name of the task
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the task
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Priority level of the task
    /// </summary>
    [JsonProperty("priority")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Priority Priority { get; set; }

    /// <summary>
    /// Current status of the task
    /// </summary>
    [JsonProperty("status")]
    [JsonConverter(typeof(StringEnumConverter))]
    public CurrentTaskStatus Status { get; set; }

    /// <summary>
    /// Optional deadline for the task completion
    /// </summary>
    [JsonProperty("deadline")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// Date and time when the task was created
    /// </summary>
    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Collection of users assigned to this task
    /// </summary>
    [JsonProperty("assignees")]
    public IEnumerable<TaskAssigneeDto> Assignees { get; set; } = new List<TaskAssigneeDto>();

    /// <summary>
    /// Collection of comments on this task
    /// </summary>
    [JsonProperty("comments")]
    public IEnumerable<CommentReadDto> Comments { get; set; } = new List<CommentReadDto>();

    /// <summary>
    /// The list containing this task
    /// </summary>
    [JsonProperty("list")]
    public ListReadDto? List { get; set; }

    /// <summary>
    /// The project containing this task
    /// </summary>
    [JsonProperty("project")]
    public ProjectReadDto? Project { get; set; }

    /// <summary>
    /// Creates a TaskReadDto from a WorkTask entity
    /// </summary>
    /// <param name="task">The task entity to convert</param>
    /// <returns>A new TaskReadDto populated with the entity's data</returns>
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

/// <summary>
/// Data transfer object for creating a new task
/// </summary>
public class TaskCreateDto {
    /// <summary>
    /// Name of the task (Required, 3-100 characters)
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Detailed description of the task (Optional, 3-100 characters when provided)
    /// </summary>
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Initial status of the task (Required)
    /// </summary>
    [Required]
    [EnumDataType(typeof(CurrentTaskStatus))]
    [JsonProperty("status")]
    public CurrentTaskStatus Status { get; set; }

    /// <summary>
    /// Priority level of the task
    /// </summary>
    [EnumDataType(typeof(Priority))]
    [JsonProperty("priority")]
    public Priority Priority { get; set; }

    /// <summary>
    /// Optional deadline for task completion
    /// </summary>
    [DataType(DataType.DateTime)]
    [JsonProperty("deadline")]
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// ID of the project this task belongs to (Set internally)
    /// </summary>
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// ID of the list this task belongs to (Set internally)
    /// </summary>
    [JsonIgnore]
    public Guid ListId { get; set; }

    /// <summary>
    /// Converts the DTO to a WorkTask entity
    /// </summary>
    /// <returns>A new WorkTask entity populated with the DTO's data</returns>
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

/// <summary>
/// Data transfer object for updating an existing task
/// </summary>
public class TaskUpdateDto {
    /// <summary>
    /// Updated name for the task (3-100 characters when provided)
    /// </summary>
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Updated description for the task (3-100 characters when provided)
    /// </summary>
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Updated status for the task
    /// </summary>
    [EnumDataType(typeof(CurrentTaskStatus))]
    [JsonProperty("status")]
    public CurrentTaskStatus Status { get; set; }

    /// <summary>
    /// Updated priority level for the task
    /// </summary>
    [EnumDataType(typeof(Priority))]
    [JsonProperty("priority")]
    public Priority Priority { get; set; }

    /// <summary>
    /// Updated deadline for the task
    /// </summary>
    [DataType(DataType.DateTime)]
    [JsonProperty("deadline")]
    public DateTime Deadline { get; set; }

    /// <summary>
    /// Updates an existing WorkTask entity with the DTO's data
    /// </summary>
    /// <param name="entity">The entity to update</param>
    public void UpdateEntity(WorkTask entity) {
        entity.Name = Name ?? entity.Name;
        entity.Description = Description ?? entity.Description;
        entity.Status = Status;
        entity.Priority = Priority;
        entity.Deadline = Deadline;
    }
}
