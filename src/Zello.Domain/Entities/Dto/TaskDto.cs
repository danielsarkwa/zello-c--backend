using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zello.Domain.Enums;

namespace Zello.Domain.Entities.Dto;

/// <summary>
/// Data transfer object for task entity with full details including relationships
/// </summary>
/// <example>
/// {
///     "id": "123e4567-e89b-12d3-a456-426614174000",
///     "name": "Implement feature",
///     "description": "Implement new feature X",
///     "status": "InProgress",
///     "priority": "High",
///     "deadline": "2024-02-01T00:00:00Z",
///     "created_date": "2024-01-01T12:00:00Z",
///     "updated_date": "2024-01-15T12:00:00Z",
///     "project_id": "123e4567-e89b-12d3-a456-426614174001",
///     "list_id": "123e4567-e89b-12d3-a456-426614174002",
///     "assignees": [],
///     "comments": []
/// }
/// </example>
[Table("tasks")]
public class TaskDto {
    /// <summary>
    /// Unique identifier of the task
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [Key]
    [Column("id")]
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the task (maximum 100 characters)
    /// </summary>
    /// <example>Implement feature</example>
    [Required]
    [Column("name")]
    [MaxLength(100)]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the task (maximum 500 characters)
    /// </summary>
    /// <example>Implement new feature X with specific requirements</example>
    [Column("description")]
    [MaxLength(500)]
    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the task
    /// </summary>
    /// <remarks>
    /// Possible values:
    /// - ToDo: Task not yet started
    /// - InProgress: Task is being worked on
    /// - Done: Task is completed
    /// - Blocked: Task is blocked by dependencies
    /// </remarks>
    /// <example>InProgress</example>
    [Column("status")]
    [JsonProperty("status")]
    public CurrentTaskStatus Status { get; set; }

    /// <summary>
    /// Priority level of the task
    /// </summary>
    /// <remarks>
    /// Possible values:
    /// - Low: Non-urgent tasks
    /// - Medium: Standard priority
    /// - High: Urgent tasks
    /// - Critical: Immediate attention required
    /// </remarks>
    /// <example>High</example>
    [Column("priority")]
    [JsonProperty("priority")]
    public Priority Priority { get; set; }

    /// <summary>
    /// Optional deadline for the task completion
    /// </summary>
    /// <example>2024-02-01T00:00:00Z</example>
    [Column("deadline")]
    [JsonProperty("deadline", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? Deadline { get; set; }

    /// <summary>
    /// Timestamp when the task was created
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    [Column("created_date")]
    [JsonProperty("created_date")]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the task was last updated
    /// </summary>
    /// <example>2024-01-15T12:00:00Z</example>
    [Column("updated_date")]
    [JsonProperty("updated_date", NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(IsoDateTimeConverter))]
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// ID of the project this task belongs to
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    [Column("project_id")]
    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// ID of the list this task belongs to within the project
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174002</example>
    [Column("list_id")]
    [JsonProperty("list_id")]
    public Guid ListId { get; set; }

    /// <summary>
    /// Navigation property to the associated project
    /// </summary>
    /// <remarks>
    /// This property is ignored in JSON serialization
    /// </remarks>
    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual ProjectDto Project { get; set; } = null!;

    /// <summary>
    /// Navigation property to the associated list
    /// </summary>
    /// <remarks>
    /// This property is ignored in JSON serialization
    /// </remarks>
    [ForeignKey("ListId")]
    [JsonIgnore]
    public virtual ListDto List { get; set; } = null!;

    /// <summary>
    /// Collection of users assigned to this task
    /// </summary>
    /// <remarks>
    /// Contains TaskAssigneeDto objects representing task assignments
    /// </remarks>
    [JsonProperty("assignees")]
    public virtual ICollection<TaskAssigneeDto> Assignees { get; set; } =
        new List<TaskAssigneeDto>();

    /// <summary>
    /// Collection of comments made on this task
    /// </summary>
    /// <remarks>
    /// Contains CommentDto objects representing task comments
    /// </remarks>
    [JsonProperty("comments")]
    public virtual ICollection<CommentDto> Comments { get; set; } = new List<CommentDto>();
}
