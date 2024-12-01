using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Application.Features.Tasks.Models;

/// <summary>
/// Request model for assigning a user to a task
/// </summary>
/// <example>
/// {
///     "user_id": "123e4567-e89b-12d3-a456-426614174000"
/// }
/// </example>
public class AssignUserRequest {
    /// <summary>
    /// ID of the user to assign to the task
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    [JsonProperty("user_id")]
    [Required]
    public Guid UserId { get; set; }
}
