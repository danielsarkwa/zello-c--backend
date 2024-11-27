using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;

namespace Zello.Application.Dtos;

public class WorkspaceReadDto {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public DateTime CreatedDate { get; set; }

    // Including Projects and Members in the Read DTO
    public IEnumerable<ProjectReadDto> Projects { get; set; } = new List<ProjectReadDto>();

    public IEnumerable<WorkspaceMemberReadDto> Members { get; set; } = new List<WorkspaceMemberReadDto>();

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

public class WorkspaceCreateDto {
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
        };
    }
}

public class WorkspaceUpdateDto {
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("name")]
    public required string Name { get; set; }

    public Workspace ToEntity(Workspace workspace) {
        workspace.Name = Name;
        return workspace;
    }
}
