using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object for reading workspace member information.
/// Represents a user's membership and access level within a workspace.
/// </summary>
/// <example>
/// {
///     "id": "123e4567-e89b-12d3-a456-426614174010",
///     "workspace_id": "123e4567-e89b-12d3-a456-426614174011",
///     "user_id": "123e4567-e89b-12d3-a456-426614174012",
///     "access_level": "Member",
///     "created_date": "2024-01-01T12:00:00Z"
/// }
/// </example>
public class WorkspaceMemberReadDto {
    /// <summary>
    /// The unique identifier of the workspace membership
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174010</example>
    [JsonProperty("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// The workspace ID this membership belongs to
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174011</example>
    [JsonProperty("workspace_id")]
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// The user ID of the workspace member
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174012</example>
    [JsonProperty("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// The access level granted to the member in this workspace
    /// </summary>
    /// <remarks>
    /// Possible values:
    /// - Guest: Limited access to view and comment
    /// - Member: Standard access to create and edit
    /// - Owner: Full access including member management
    /// - Admin: System-wide administrative access
    /// </remarks>
    /// <example>Member</example>
    [JsonProperty("access_level")]
    public AccessLevel AccessLevel { get; set; }

    /// <summary>
    /// The UTC datetime when the membership was created
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Converts a WorkspaceMember entity to a WorkspaceMemberReadDto
    /// </summary>
    /// <param name="member">The WorkspaceMember entity to convert</param>
    /// <returns>A new WorkspaceMemberReadDto containing the member's information, or a default DTO if the input is null</returns>
    /// <remarks>
    /// If the input member is null, returns a new DTO with default values.
    /// This helps prevent null reference exceptions in the API response.
    /// </remarks>
    public static WorkspaceMemberReadDto FromEntity(WorkspaceMember? member) {
        if (member == null) {
            return new WorkspaceMemberReadDto(); // Returns a DTO with default values
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
/// Data transfer object for creating a new workspace member
/// </summary>
/// <example>
/// {
///     "user_id": "123e4567-e89b-12d3-a456-426614174012",
///     "role": "Member"
/// }
/// </example>
public class WorkspaceMemberCreateDto {
    /// <summary>
    /// The user ID to add as a workspace member
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174012</example>
    [Required]
    [JsonProperty("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// The access level to grant to the new member
    /// </summary>
    /// <remarks>
    /// Must be a valid AccessLevel enum value:
    /// - Guest: Limited access to view and comment
    /// - Member: Standard access to create and edit
    /// - Owner: Full access including member management
    /// - Admin: System-wide administrative access
    /// </remarks>
    /// <example>Member</example>
    [Required]
    [EnumDataType(typeof(AccessLevel), ErrorMessage = "Invalid role type.")]
    [JsonProperty("role")]
    public AccessLevel AccessLevel { get; set; }

    /// <summary>
    /// Converts the DTO to a WorkspaceMember entity
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace this member will belong to</param>
    /// <returns>A new WorkspaceMember entity</returns>
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
/// Data transfer object for updating a workspace member's role
/// </summary>
/// <example>
/// {
///     "role": "Owner"
/// }
/// </example>
public class WorkspaceMemberUpdateDto {
    /// <summary>
    /// The new access level to assign to the member
    /// </summary>
    /// <remarks>
    /// Must be a valid AccessLevel enum value:
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

    /// <summary>
    /// Updates an existing WorkspaceMember entity with the new access level
    /// </summary>
    /// <param name="workspaceMember">The workspace member entity to update</param>
    /// <returns>The updated WorkspaceMember entity</returns>
    public WorkspaceMember ToEntity(WorkspaceMember workspaceMember) {
        workspaceMember.AccessLevel = Role;
        return workspaceMember;
    }
}
