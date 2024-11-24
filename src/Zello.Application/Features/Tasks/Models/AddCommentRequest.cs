using System.ComponentModel.DataAnnotations;

namespace Zello.Application.Features.Tasks.Models;


public sealed record AddCommentRequest(
    [Required]
    [MaxLength(500)]
    string Content = ""
);
