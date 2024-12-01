using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Enums;

namespace Zello.Application.Dtos;

public class ProjectReadDto {
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("start_date")]
    public DateTime? StartDate { get; set; }

    [JsonProperty("end_date")]
    public DateTime? EndDate { get; set; }

    [JsonProperty("status")]
    public ProjectStatus Status { get; set; }

    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("members")]
    public IEnumerable<ProjectMemberReadDto> Members { get; set; } =
        new List<ProjectMemberReadDto>();

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

public class ProjectCreateDto {
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 3)]
    [JsonProperty("description")]
    public string? Description { get; set; }

    [Required]
    [JsonProperty("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    [JsonProperty("start_date")] // Changed to match snake_case
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    [JsonProperty("end_date")] // Changed to match snake_case
    public DateTime? EndDate { get; set; }

    [Required]
    [JsonProperty("status")]
    public ProjectStatus Status { get; set; } = ProjectStatus.NotStarted;

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
