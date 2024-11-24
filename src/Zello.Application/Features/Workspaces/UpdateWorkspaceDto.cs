using System.ComponentModel.DataAnnotations;
using Zello.Domain.Entities.Dto;

namespace Zello.Application.Features.Workspaces;

/// <summary>
/// Command DTO for workspace update requests
/// </summary>
public record UpdateWorkspaceDto(
    /// <summary>
    /// The new name of the workspace
    /// </summary>
    [Required]
    [MaxLength(100)]
    string Name = "",
    /// <summary>
    /// Optional description of the workspace
    /// </summary>
    [MaxLength(500)]
    string? Description = null) {
    /// <summary>
    /// Updates the existing workspace with new values while preserving other properties
    /// </summary>
    /// <param name="workspace">The workspace to update</param>
    public void UpdateWorkspace(WorkspaceDto workspace) {
        ArgumentNullException.ThrowIfNull(workspace);

        workspace.Name = Name;
        // Only update description if it's provided in the DTO
        if (Description != null) {
            workspace.Description = Description;
        }
        // Preserve all other properties (Id, OwnerId, CreatedDate, Projects, Members)
    }
}
