using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities.Api.User;
namespace Zello.Application.Dtos;
/// <summary>
/// Data transfer object for elevating a member's access level
/// </summary>
/// <example>
/// {
///     "member_id": "123e4567-e89b-12d3-a456-426614174012",
///     "new_access_level": "Owner"
/// }
/// </example>
public class MemberElevationDto {
    /// <summary>
    /// The ID of the member to elevate
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174012</example>
    [Required]
    [JsonProperty("member_id")]
    public Guid MemberId { get; set; }
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
    [EnumDataType(typeof(AccessLevel), ErrorMessage = "Invalid access level.")]
    [JsonProperty("new_access_level")]
    public AccessLevel NewAccessLevel { get; set; }
}
