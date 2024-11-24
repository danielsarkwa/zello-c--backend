using System.ComponentModel.DataAnnotations;

namespace Zello.Application.Features.Comments.Models;

public class CreateCommentRequest {
    public CreateCommentRequest() {
        Content = string.Empty;
    }

    public Guid TaskId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Content { get; set; }
}
