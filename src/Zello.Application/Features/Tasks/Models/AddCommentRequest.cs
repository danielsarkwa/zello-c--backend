using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Application.Features.Tasks.Models;

/// <summary>
/// Request model for adding a comment to a task
/// </summary>
/// <example>
/// {
///     "content": "This is a comment on the task"
/// }
/// </example>
public class AddCommentRequest {
    /// <summary>
    /// Content of the comment (maximum 500 characters)
    /// </summary>
    /// <example>This is a comment on the task</example>
    [Required]
    [StringLength(500)]
    [JsonProperty("content")]
    public required string Content { get; set; }
}
