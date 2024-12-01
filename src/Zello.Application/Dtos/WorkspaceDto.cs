using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object for reading workspace information
/// </summary>
/// <example>
/// {
///     "id": "123e4567-e89b-12d3-a456-426614174000",
///     "name": "Development Team",
///     "ownerId": "123e4567-e89b-12d3-a456-426614174001",
///     "createdDate": "2024-01-01T12:00:00Z",
///     "projects": [],
///     "members": []
/// }
/// </example>
public class WorkspaceReadDto {
    /// <summary>
    /// Unique identifier of the workspace
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the workspace
    /// </summary>
    /// <example>Development Team</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who owns the workspace
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Date when the workspace was created
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// List of projects within the workspace
    /// </summary>
    public IEnumerable<ProjectReadDto> Projects { get; set; } = new List<ProjectReadDto>();

    /// <summary>
    /// List of members with access to the workspace
    /// </summary>
    public IEnumerable<WorkspaceMemberReadDto> Members { get; set; } =
        new List<WorkspaceMemberReadDto>();

    public static WorkspaceReadDto FromEntity(Workspace workspace) {
        return new WorkspaceReadDto {
            Id = workspace.Id,
            Name = workspace.Name,
            OwnerId = workspace.OwnerId,
            CreatedDate = workspace.CreatedDate,
            Projects = workspace.Projects.Select(ProjectReadDto.FromEntity).ToList(),
            Members = workspace.Members.Select(WorkspaceMemberReadDto.FromEntity).ToList()
        };
    }
}

/// <summary>
/// Data transfer object for creating a new workspace
/// </summary>
/// <example>
/// {
///     "name": "New Development Team"
/// }
/// </example>
public class WorkspaceCreateDto {
    /// <summary>
    /// Name of the workspace (3-20 characters)
    /// </summary>
    /// <example>New Development Team</example>
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    public Workspace ToEntity(Guid ownerId) {
        return new Workspace {
            Id = Guid.NewGuid(),
            Name = Name,
            OwnerId = ownerId,
            CreatedDate = DateTime.UtcNow,
            Projects = new List<Project>(),
            Members = new List<WorkspaceMember> {
                new WorkspaceMember {
                    UserId = ownerId,
                    AccessLevel = AccessLevel.Owner
                }
            }
        };
    }
}

/// <summary>
/// Data transfer object for updating an existing workspace
/// </summary>
/// <example>
/// {
///     "name": "Updated Team Name"
/// }
/// </example>
public class WorkspaceUpdateDto {
    /// <summary>
    /// Updated name for the workspace (3-20 characters)
    /// </summary>
    /// <example>Updated Team Name</example>
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    public Workspace ToEntity(Workspace workspace) {
        workspace.Name = Name;
        return workspace;
    }
}
