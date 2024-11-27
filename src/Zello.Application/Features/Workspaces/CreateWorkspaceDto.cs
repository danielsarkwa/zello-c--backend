using System.ComponentModel.DataAnnotations;
using Zello.Domain.Entities.Dto;

namespace Zello.Application.Features.Workspaces;

/// <summary>
/// Command DTO for workspace creation requests
/// </summary>
public record CreateWorkspaceDto(
    [Required]
    [MaxLength(100)]
    string Name = "",
    [MaxLength(500)]
    string? Description = null) {
    public WorkspaceDto ToWorkspaceDto() {
        return new WorkspaceDto {
            Name = Name,
            Description = Description,
            CreatedDate = DateTime.UtcNow,
            Projects = new List<ProjectDto>(),
            Members = new List<WorkspaceMemberDto>()
        };
    }
}
