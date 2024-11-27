using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Dtos;

public class ProjectMemberReadDto {
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    [JsonProperty("workspace_member_id")]
    public Guid WorkspaceMemberId { get; set; }

    [JsonProperty("access_level")]
    public AccessLevel AccessLevel { get; set; }

    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; }

    // Keep only WorkspaceMember, remove Project to break circular reference
    [JsonProperty("workspace_member")]
    public WorkspaceMemberReadDto WorkspaceMember { get; set; } = new WorkspaceMemberReadDto();

    public static ProjectMemberReadDto FromEntity(ProjectMember projectMember) {
        return new ProjectMemberReadDto {
            Id = projectMember.Id,
            ProjectId = projectMember.ProjectId,
            WorkspaceMemberId = projectMember.WorkspaceMemberId,
            AccessLevel = projectMember.AccessLevel,
            CreatedDate = projectMember.CreatedDate,
            WorkspaceMember = WorkspaceMemberReadDto.FromEntity(projectMember.WorkspaceMember)
        };
    }
}

public class ProjectMemberCreateDto {
    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    [JsonProperty("workspace_member_id")]
    public Guid WorkspaceMemberId { get; set; }

    [JsonProperty("access_level")]
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Member;

    public ProjectMember ToEntity() {
        return new ProjectMember {
            Id = Guid.NewGuid(),
            ProjectId = ProjectId,
            WorkspaceMemberId = WorkspaceMemberId,
            AccessLevel = AccessLevel,
            CreatedDate = DateTime.UtcNow
        };
    }
}

public class ProjectMemberUpdateDto {
    [Required]
    [EnumDataType(typeof(AccessLevel), ErrorMessage = "Invalid role type.")]
    [JsonProperty("access_level")]
    public AccessLevel AccessLevel { get; set; }

    public ProjectMember ToEntity(ProjectMember projectMember) {
        projectMember.AccessLevel = AccessLevel;
        return projectMember;
    }
}
