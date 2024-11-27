using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;

namespace Zello.Application.Dtos;

public class CommentReadDto {
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public required string Content { get; set; }
    public DateTime CreatedDate { get; set; }

    // Only keep the User related entity
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

    public Comment ToEntity() {
        return new Comment {
            UserId = UserId,
            TaskId = TaskId,
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
