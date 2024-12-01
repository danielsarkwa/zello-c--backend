using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object for reading project member information
/// </summary>
public class ProjectMemberReadDto {
    /// <summary>
    /// Unique identifier of the project membership
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the project
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    [JsonProperty("project_id")]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// ID of the workspace member
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174002</example>
    [JsonProperty("workspace_member_id")]
    public Guid WorkspaceMemberId { get; set; }

    /// <summary>
    /// Access level of the member in the project
    /// </summary>
    /// <example>Member</example>
    [JsonProperty("access_level")]
    public AccessLevel AccessLevel { get; set; }

    /// <summary>
    /// Date when the member was added to the project
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Detailed workspace member information
    /// </summary>
    [JsonProperty("workspace_member")]
    public WorkspaceMemberReadDto? WorkspaceMember { get; set; }

    public static ProjectMemberReadDto FromEntity(ProjectMember projectMember) {
        return new ProjectMemberReadDto {
            Id = projectMember.Id,
            ProjectId = projectMember.ProjectId,
            WorkspaceMemberId = projectMember.WorkspaceMemberId,
            AccessLevel = projectMember.AccessLevel,
            CreatedDate = projectMember.CreatedDate,
            WorkspaceMember = projectMember.WorkspaceMember != null
                ? WorkspaceMemberReadDto.FromEntity(projectMember.WorkspaceMember)
                : null
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
