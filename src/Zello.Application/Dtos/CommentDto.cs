using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;

namespace Zello.Application.Dtos;

/// <summary>
/// Data transfer object for reading task comments
/// </summary>
/// <example>
/// {
///     "id": "123e4567-e89b-12d3-a456-426614174000",
///     "taskId": "123e4567-e89b-12d3-a456-426614174001",
///     "userId": "123e4567-e89b-12d3-a456-426614174002",
///     "content": "This is a comment",
///     "createdDate": "2024-01-01T12:00:00Z",
///     "user": { ... }
/// }
/// </example>
public class CommentReadDto {
    /// <summary>
    /// Unique identifier of the comment
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the task this comment belongs to
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174001</example>
    public Guid TaskId { get; set; }

    /// <summary>
    /// ID of the user who created the comment
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174002</example>
    public Guid UserId { get; set; }

    /// <summary>
    /// Content of the comment
    /// </summary>
    /// <example>This is a comment</example>
    public required string Content { get; set; }

    /// <summary>
    /// Date when the comment was created
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Detailed information about the comment author
    /// </summary>
    public UserReadDto User { get; set; } = new UserReadDto();

    public static CommentReadDto FromEntity(Comment comment) {
        return new CommentReadDto {
            Id = comment.Id,
            TaskId = comment.TaskId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedDate = comment.CreatedDate,
            User = UserReadDto.FromEntity(comment.User)
        };
    }
}

public class CommentCreateDto {
    [Required]
    [JsonProperty("userId")]
    public Guid UserId { get; set; }

    [Required]
    [JsonProperty("taskId")]
    public Guid TaskId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 3)]
    [JsonProperty("content")]
    public required string Content { get; set; }

    public Comment ToEntity(Guid UserId, Guid? RequstTaskId) {
        return new Comment {
            Id = Guid.NewGuid(),
            UserId = UserId,
            TaskId = RequstTaskId ?? TaskId,
            Content = Content,
            CreatedDate = DateTime.UtcNow
        };
    }
}

public class CommentUpdateDto {
    [StringLength(500, MinimumLength = 3)]
    [JsonProperty("content")]
    public required string Content { get; set; }

    public Comment ToEntity(Comment comment) {
        comment.Content = Content;
        return comment;
    }
}
