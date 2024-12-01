using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Enums;

namespace Zello.Application.Dtos;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zello.Domain.Entities.Dto;
using Zello.Domain.Enums;

/// <summary>
/// Data transfer object for reading task information
/// </summary>
public class TaskReadDto {
    /// <summary>
    /// Unique identifier of the task
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the project containing this task
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// ID of the list containing this task
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174002</example>
    [JsonProperty("list_id")]
    public Guid ListId { get; set; }

    /// <summary>
    /// Name of the task
    /// </summary>
    /// <example>Implement user authentication</example>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the task
    /// </summary>
    /// <example>Set up JWT authentication with refresh tokens</example>
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Priority level of the task
    /// </summary>
    /// <example>High</example>
    [JsonProperty("priority")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Priority Priority { get; set; }

    /// <summary>
    /// Current status of the task
    /// </summary>
    /// <example>InProgress</example>
    [JsonProperty("status")]
    [JsonConverter(typeof(StringEnumConverter))]
    public CurrentTaskStatus Status { get; set; }

    /// <summary>
    /// Optional deadline for the task
    /// </summary>
    /// <example>2024-01-15T00:00:00Z</example>
    [JsonProperty("deadline")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// Date when the task was created
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// List of users assigned to this task
    /// </summary>
    [JsonProperty("assignees")]
    public IEnumerable<TaskAssigneeReadDto> Assignees { get; set; } =
        new List<TaskAssigneeReadDto>();

    /// <summary>
    /// Comments made on this task
    /// </summary>
    [JsonProperty("comments")]
    public IEnumerable<CommentReadDto> Comments { get; set; } = new List<CommentReadDto>();

    /// <summary>
    /// List containing this task
    /// </summary>
    [JsonProperty("list")]
    public ListReadDto? List { get; set; }

    /// <summary>
    /// Project containing this task
    /// </summary>
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
            Assignees = task.Assignees.Select(a => new TaskAssigneeReadDto {
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
/// <example>
/// {
///     "name": "Implement login",
///     "description": "Add user authentication",
///     "status": "ToDo",
///     "priority": "High",
///     "deadline": "2024-02-01T00:00:00Z",
///     "project_id": "123e4567-e89b-12d3-a456-426614174000",
///     "list_id": "123e4567-e89b-12d3-a456-426614174001"
/// }
/// </example>
public class TaskCreateDto {
    /// <summary>
    /// Name of the task (3-100 characters)
    /// </summary>
    /// <example>Implement login</example>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Detailed description of the task (3-100 characters)
    /// </summary>
    /// <example>Add user authentication with JWT tokens</example>
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Initial status of the task
    /// </summary>
    /// <example>ToDo</example>
    [Required]
    [JsonProperty("status")]
    [JsonConverter(typeof(StringEnumConverter))]
    public CurrentTaskStatus Status { get; set; }

    /// <summary>
    /// Priority level of the task
    /// </summary>
    /// <example>High</example>
    [JsonProperty("priority")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Priority Priority { get; set; }

    /// <summary>
    /// Optional deadline for the task
    /// </summary>
    /// <example>2024-02-01T00:00:00Z</example>
    [JsonProperty("deadline")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// ID of the project this task belongs to
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [Required]
    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// ID of the list this task belongs to
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    [Required]
    [JsonProperty("list_id")]
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

/// <summary>
/// Data transfer object for updating an existing task
/// </summary>
/// <example>
/// {
///     "name": "Updated task name",
///     "description": "Updated description",
///     "status": "InProgress",
///     "priority": "High",
///     "deadline": "2024-02-15T00:00:00Z"
/// }
/// </example>
public class TaskUpdateDto {
    /// <summary>
    /// Updated name of the task (3-100 characters)
    /// </summary>
    /// <example>Updated task name</example>
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Updated description of the task (3-100 characters)
    /// </summary>
    /// <example>Updated task description with new requirements</example>
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Updated status of the task
    /// </summary>
    /// <example>InProgress</example>
    [EnumDataType(typeof(CurrentTaskStatus))]
    [JsonProperty("status")]
    public CurrentTaskStatus Status { get; set; }

    /// <summary>
    /// Updated priority level of the task
    /// </summary>
    /// <example>High</example>
    [EnumDataType(typeof(Priority))]
    [JsonProperty("priority")]
    public Priority Priority { get; set; }

    /// <summary>
    /// Updated deadline for the task
    /// </summary>
    /// <example>2024-02-15T00:00:00Z</example>
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
