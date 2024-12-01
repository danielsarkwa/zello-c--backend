using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Dtos;

public class WorkspaceMemberReadDto {
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("workspace_id")]
    public Guid WorkspaceId { get; set; }

    [JsonProperty("user_id")]
    public Guid UserId { get; set; }

    [JsonProperty("access_level")]
    public AccessLevel AccessLevel { get; set; }

    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; }

    public static WorkspaceMemberReadDto FromEntity(WorkspaceMember? member) {
        if (member == null) {
            throw new ArgumentNullException(nameof(member), "WorkspaceMember cannot be null");
        }

        return new WorkspaceMemberReadDto {
            Id = member.Id,
            WorkspaceId = member.WorkspaceId,
            UserId = member.UserId,
            AccessLevel = member.AccessLevel,
            CreatedDate = member.CreatedDate
        };
    }
}

public class WorkspaceMemberCreateDto {
    [Required]
    [JsonProperty("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [JsonProperty("role")]
    public AccessLevel AccessLevel { get; set; }

    public WorkspaceMember ToEntity(Guid workspaceId) {
        return new WorkspaceMember {
            WorkspaceId = workspaceId,
            UserId = UserId,
            AccessLevel = AccessLevel,
            CreatedDate = DateTime.UtcNow
        };
    }
}

public class WorkspaceMemberUpdateDto {
    [Required]
    [EnumDataType(typeof(AccessLevel), ErrorMessage = "Invalid role type.")]
    [JsonProperty("role")]
    public AccessLevel Role { get; set; }

    public WorkspaceMember ToEntity(WorkspaceMember workspaceMember) {
        workspaceMember.AccessLevel = Role;
        return workspaceMember;
    }
}
