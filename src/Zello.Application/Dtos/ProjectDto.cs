using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Enums;

namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object for reading project information
/// </summary>
public class ProjectReadDto {
    /// <summary>
    /// Unique identifier of the project
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the workspace containing this project
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    [JsonProperty("workspace_id")]
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// Name of the project
    /// </summary>
    /// <example>Project Alpha</example>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the project
    /// </summary>
    /// <example>This project aims to improve customer satisfaction</example>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Planned start date of the project
    /// </summary>
    /// <example>2024-01-01T00:00:00Z</example>
    [JsonProperty("start_date")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Expected end date of the project
    /// </summary>
    /// <example>2024-12-31T00:00:00Z</example>
    [JsonProperty("end_date")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Current status of the project
    /// </summary>
    /// <example>InProgress</example>
    [JsonProperty("status")]
    public ProjectStatus Status { get; set; }

    /// <summary>
    /// Date when the project was created
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// List of project members with their access levels
    /// </summary>
    [JsonProperty("members")]
    public IEnumerable<ProjectMemberReadDto> Members { get; set; } =
        new List<ProjectMemberReadDto>();

    /// <summary>
    /// Lists containing tasks within the project
    /// </summary>
    [JsonProperty("lists")]
    public IEnumerable<ListReadDto> Lists { get; set; } = new List<ListReadDto>();

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
public class ProjectCreateDto {
    /// <summary>
    /// Name of the project (3-20 characters)
    /// </summary>
    /// <example>New Project</example>
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the project (3-100 characters)
    /// </summary>
    /// <example>This is a new project for Q1 2024</example>
    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// ID of the workspace where the project will be created
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [Required]
    [JsonProperty("workspace_id")]
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// Planned start date of the project
    /// </summary>
    /// <example>2024-01-01</example>
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    [JsonProperty("start_date")]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Expected end date of the project
    /// </summary>
    /// <example>2024-12-31</example>
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    [JsonProperty("end_date")]
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Initial status of the project
    /// </summary>
    /// <example>NotStarted</example>
    [Required]
    [JsonProperty("status")]
    public ProjectStatus Status { get; set; }

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

public class ProjectUpdateDto {
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public string? Name { get; set; }

    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    [JsonProperty("startDate")]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    [JsonProperty("endDate")]
    public DateTime EndDate { get; set; }

    [JsonProperty("status")]
    public ProjectStatus Status { get; set; }

    public Project ToEntity(Project project) {
        project.Name = Name ?? project.Name;
        project.Description = Description ?? project.Description;
        project.StartDate = StartDate;
        project.EndDate = EndDate;
        project.Status = Status;
        return project;
    }
}
