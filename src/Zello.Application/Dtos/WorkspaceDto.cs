using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;

namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object for reading workspace information
/// </summary>
/// <example>
/// {
///     "id": "123e4567-e89b-12d3-a456-426614174010",
///     "name": "Engineering Team",
///     "ownerId": "123e4567-e89b-12d3-a456-426614174000",
///     "createdDate": "2024-01-01T12:00:00Z",
///     "projects": [],
///     "members": []
/// }
/// </example>
public class WorkspaceReadDto {
    /// <summary>
    /// The unique identifier of the workspace
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174010</example>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the workspace
    /// </summary>
    /// <example>Engineering Team</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the workspace owner
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// The UTC datetime when the workspace was created
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// List of projects associated with this workspace
    /// </summary>
    public IEnumerable<ProjectReadDto> Projects { get; set; } = new List<ProjectReadDto>();

    /// <summary>
    /// List of members associated with this workspace
    /// </summary>
    public IEnumerable<WorkspaceMemberReadDto> Members { get; set; } =
        new List<WorkspaceMemberReadDto>();

    /// <summary>
    /// Creates a WorkspaceReadDto from a Workspace entity
    /// </summary>
    /// <param name="workspace">The workspace entity to convert</param>
    /// <returns>A new WorkspaceReadDto instance</returns>
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
///     "name": "New Engineering Team"
/// }
/// </example>
public class WorkspaceCreateDto {
    /// <summary>
    /// The name of the workspace to create
    /// </summary>
    /// <remarks>
    /// Must be between 3 and 20 characters long
    /// </remarks>
    /// <example>New Engineering Team</example>
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Converts the DTO to a Workspace entity
    /// </summary>
    /// <param name="ownerId">The ID of the user who will own the workspace</param>
    /// <returns>A new Workspace entity</returns>
    public Workspace ToEntity(Guid ownerId) {
        return new Workspace {
            Id = Guid.NewGuid(),
            Name = Name,
            OwnerId = ownerId,
            CreatedDate = DateTime.UtcNow,
        };
    }
}

/// <summary>
/// Data transfer object for updating an existing workspace
/// </summary>
/// <example>
/// {
///     "name": "Updated Engineering Team"
/// }
/// </example>
public class WorkspaceUpdateDto {
    /// <summary>
    /// The new name for the workspace
    /// </summary>
    /// <remarks>
    /// Must be between 3 and 20 characters long
    /// </remarks>
    /// <example>Updated Engineering Team</example>
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Updates an existing Workspace entity with the new data
    /// </summary>
    /// <param name="workspace">The workspace entity to update</param>
    /// <returns>The updated Workspace entity</returns>
    public Workspace ToEntity(Workspace workspace) {
        workspace.Name = Name;
        return workspace;
    }
}
