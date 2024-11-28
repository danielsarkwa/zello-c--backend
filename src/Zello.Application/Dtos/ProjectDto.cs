using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Enums;

namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object for reading project information, including its members and task lists
/// </summary>
/// <example>
/// {
///     "id": "123e4567-e89b-12d3-a456-426614174010",
///     "workspace_id": "123e4567-e89b-12d3-a456-426614174011",
///     "name": "Mobile App Development",
///     "description": "Developing the new mobile app version",
///     "start_date": "2024-01-01T00:00:00Z",
///     "end_date": "2024-06-30T00:00:00Z",
///     "status": "InProgress",
///     "created_date": "2024-01-01T12:00:00Z",
///     "members": [
///         {
///             "id": "123e4567-e89b-12d3-a456-426614174020",
///             "project_id": "123e4567-e89b-12d3-a456-426614174010",
///             "workspace_member_id": "123e4567-e89b-12d3-a456-426614174030",
///             "access_level": "Member",
///             "created_date": "2024-01-01T12:00:00Z"
///         }
///     ],
///     "lists": [
///         {
///             "id": "123e4567-e89b-12d3-a456-426614174040",
///             "project_id": "123e4567-e89b-12d3-a456-426614174010",
///             "name": "To Do",
///             "position": 0,
///             "created_date": "2024-01-01T12:00:00Z"
///         }
///     ]
/// }
/// </example>
public class ProjectReadDto {
    /// <summary>
    /// The unique identifier of the project
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174010</example>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// The workspace ID this project belongs to
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174011</example>
    [JsonProperty("workspace_id")]
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// The name of the project
    /// </summary>
    /// <example>Mobile App Development</example>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the project
    /// </summary>
    /// <example>Developing the new mobile app version</example>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The planned start date of the project
    /// </summary>
    /// <example>2024-01-01T00:00:00Z</example>
    [JsonProperty("start_date")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// The planned end date of the project
    /// </summary>
    /// <example>2024-06-30T00:00:00Z</example>
    [JsonProperty("end_date")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Current status of the project
    /// </summary>
    /// <example>InProgress</example>
    [JsonProperty("status")]
    public ProjectStatus Status { get; set; }

    /// <summary>
    /// The UTC datetime when the project was created
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// List of project members and their roles
    /// </summary>
    [JsonProperty("members")]
    public IEnumerable<ProjectMemberReadDto> Members { get; set; } =
        new List<ProjectMemberReadDto>();

    /// <summary>
    /// List of task lists in the project
    /// </summary>
    [JsonProperty("lists")]
    public IEnumerable<ListReadDto> Lists { get; set; } = new List<ListReadDto>();

    /// <summary>
    /// Converts a Project entity to a ProjectReadDto
    /// </summary>
    /// <param name="project">The Project entity to convert</param>
    /// <returns>A new ProjectReadDto containing the project's information</returns>
    public static ProjectReadDto FromEntity(Project project) {
        return new ProjectReadDto {
            Id = project.Id,
            WorkspaceId = project.WorkspaceId,
            Name = project.Name,
            Description = project.Description,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status,
            CreatedDate = project.CreatedDate,
            Members = project.Members.Select(m => new ProjectMemberReadDto {
                Id = m.Id,
                ProjectId = m.ProjectId,
                WorkspaceMemberId = m.WorkspaceMemberId,
                AccessLevel = m.AccessLevel,
                CreatedDate = m.CreatedDate
            }).ToList(),
            Lists = project.Lists.Select(ListReadDto.FromEntity).ToList()
        };
    }
}

/// <summary>
/// Data transfer object for creating a new project
/// </summary>
/// <example>
/// {
///     "name": "New Mobile App",
///     "description": "Development of new mobile application",
///     "workspace_id": "123e4567-e89b-12d3-a456-426614174011",
///     "start_date": "2024-01-01T00:00:00Z",
///     "end_date": "2024-06-30T00:00:00Z",
///     "status": "NotStarted"
/// }
/// </example>
public class ProjectCreateDto {
    /// <summary>
    /// The name of the project
    /// </summary>
    /// <remarks>Must be between 3 and 20 characters long</remarks>
    /// <example>New Mobile App</example>
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the project
    /// </summary>
    /// <remarks>When provided, must be between 3 and 100 characters long</remarks>
    /// <example>Development of new mobile application</example>
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The workspace ID where the project will be created
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174011</example>
    [Required]
    [JsonProperty("workspace_id")]
    public required Guid WorkspaceId { get; set; }

    /// <summary>
    /// The planned start date of the project
    /// </summary>
    /// <example>2024-01-01T00:00:00Z</example>
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    [JsonProperty("start_date")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// The planned end date of the project
    /// </summary>
    /// <example>2024-06-30T00:00:00Z</example>
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    [JsonProperty("end_date")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Initial status of the project
    /// </summary>
    /// <remarks>Defaults to NotStarted if not specified</remarks>
    /// <example>NotStarted</example>
    [Required]
    [JsonProperty("status")]
    public ProjectStatus Status { get; set; } = ProjectStatus.NotStarted;

    /// <summary>
    /// Converts the DTO to a Project entity
    /// </summary>
    /// <returns>A new Project entity with generated ID and creation date</returns>
    public Project ToEntity() {
        return new Project {
            Id = Guid.NewGuid(),
            Name = Name,
            Description = Description ?? string.Empty,
            WorkspaceId = WorkspaceId,
            StartDate = StartDate,
            EndDate = EndDate,
            Status = Status,
            CreatedDate = DateTime.UtcNow,
        };
    }
}

/// <summary>
/// Data transfer object for updating an existing project
/// </summary>
/// <example>
/// {
///     "name": "Updated Mobile App",
///     "description": "Updated project description",
///     "startDate": "2024-02-01T00:00:00Z",
///     "endDate": "2024-07-31T00:00:00Z",
///     "status": "InProgress"
/// }
/// </example>
public class ProjectUpdateDto {
    /// <summary>
    /// The new name for the project
    /// </summary>
    /// <remarks>When provided, must be between 3 and 20 characters long</remarks>
    /// <example>Updated Mobile App</example>
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The new description for the project
    /// </summary>
    /// <remarks>When provided, must be between 3 and 100 characters long</remarks>
    /// <example>Updated project description</example>
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The new start date for the project
    /// </summary>
    /// <example>2024-02-01T00:00:00Z</example>
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    [JsonProperty("startDate")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The new end date for the project
    /// </summary>
    /// <example>2024-07-31T00:00:00Z</example>
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    [JsonProperty("endDate")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// The new status for the project
    /// </summary>
    /// <example>InProgress</example>
    [JsonProperty("status")]
    public ProjectStatus Status { get; set; }

    /// <summary>
    /// Updates an existing Project entity with the new data
    /// </summary>
    /// <param name="project">The project entity to update</param>
    /// <returns>The updated Project entity</returns>
    public Project ToEntity(Project project) {
        project.Name = Name ?? project.Name;
        project.Description = Description ?? project.Description;
        project.StartDate = StartDate;
        project.EndDate = EndDate;
        project.Status = Status;
        return project;
    }
}
