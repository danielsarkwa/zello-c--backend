using System.ComponentModel.DataAnnotations;

namespace Zello.Application.Features.Comments.Models;

/// <summary>
/// Request model for creating a new comment.
/// </summary>
public class CreateCommentRequest {
    public CreateCommentRequest() {
        Content = string.Empty;
    }


    /// <summary>
    /// The ID of the task to add the comment to.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid TaskId { get; set; }


    /// <summary>
    /// The content of the comment. Maximum length is 500 characters.
    /// </summary>
    /// <example>This is a comment on the task</example>
    [Required]
    [MaxLength(500)]
    public string Content { get; set; }
}
