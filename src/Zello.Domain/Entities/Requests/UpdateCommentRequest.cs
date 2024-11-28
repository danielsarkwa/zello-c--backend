using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Domain.Entities.Requests;

/// <summary>
/// Request model for updating an existing comment.
/// </summary>
public class UpdateCommentRequest {
    /// <summary>
    /// The new content for the comment. Maximum length is 500 characters.
    /// </summary>
    /// <example>This is the updated comment text</example>
    [JsonProperty("content")]
    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;
}
