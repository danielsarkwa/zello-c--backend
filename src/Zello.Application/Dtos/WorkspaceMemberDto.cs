using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object for reading workspace member information
/// </summary>
/// <example>
/// {
///     "id": "123e4567-e89b-12d3-a456-426614174000",
///     "workspace_id": "123e4567-e89b-12d3-a456-426614174001",
///     "user_id": "123e4567-e89b-12d3-a456-426614174002",
///     "access_level": "Member",
///     "created_date": "2024-01-01T12:00:00Z"
/// }
/// </example>
public class WorkspaceMemberReadDto {
    /// <summary>
    /// Unique identifier of the workspace membership
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the workspace
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    [JsonProperty("workspace_id")]
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// ID of the user who is a member of the workspace
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174002</example>
    [JsonProperty("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Access level of the member in the workspace
    /// </summary>
    /// <example>Member</example>
    [JsonProperty("access_level")]
    public AccessLevel AccessLevel { get; set; }

    /// <summary>
    /// Date when the member was added to the workspace
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
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

/// <summary>
/// Data transfer object for adding a new member to a workspace
/// </summary>
/// <example>
/// {
///     "user_id": "123e4567-e89b-12d3-a456-426614174000",
///     "role": "Member"
/// }
/// </example>
public class WorkspaceMemberCreateDto {
    /// <summary>
    /// ID of the user to add to the workspace
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [Required]
    [JsonProperty("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Initial access level to assign to the member
    /// </summary>
    /// <remarks>
    /// Available access levels:
    /// - Guest: Limited access to view and comment
    /// - Member: Standard access to create and edit
    /// - Owner: Full access including member management
    /// - Admin: System-wide administrative access
    /// </remarks>
    /// <example>Member</example>
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

/// <summary>
/// Data transfer object for updating a workspace member's access level
/// </summary>
/// <example>
/// {
///     "role": "Owner"
/// }
/// </example>
public class WorkspaceMemberUpdateDto {
    /// <summary>
    /// New access level to assign to the member
    /// </summary>
    /// <remarks>
    /// Available access levels:
    /// - Guest: Limited access to view and comment
    /// - Member: Standard access to create and edit
    /// - Owner: Full access including member management
    /// - Admin: System-wide administrative access
    /// </remarks>
    /// <example>Owner</example>
    [Required]
    [EnumDataType(typeof(AccessLevel), ErrorMessage = "Invalid role type.")]
    [JsonProperty("role")]
    public AccessLevel Role { get; set; }

    public WorkspaceMember ToEntity(WorkspaceMember workspaceMember) {
        workspaceMember.AccessLevel = Role;
        return workspaceMember;
    }
}
