using Zello.Application.Dtos;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;

namespace Zello.Application.ServiceImplementations;

public class CommentService : ICommentService {
    private readonly ICommentRepository _commentRepository;
    private readonly IWorkTaskRepository _workTaskRepository;

    public CommentService(ICommentRepository commentRepository,
        IWorkTaskRepository workTaskRepository) {
        _commentRepository = commentRepository;
        _workTaskRepository = workTaskRepository;
    }

    public async Task<TaskProjectDetailsDto> GetTaskProjectDetailsAsync(Guid taskId) {
        var task = await _workTaskRepository.GetTaskByIdAsync(taskId);
        if (task == null)
            throw new Exception($"Task with ID {taskId} not found");

        return new TaskProjectDetailsDto {
            TaskId = task.Id,
            ProjectId = task.ProjectId
        };
    }


    public async Task<CommentReadDto> GetCommentByIdAsync(Guid commentId) {
        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment == null)
            throw new Exception($"Comment with ID {commentId} not found");

        return MapToCommentReadDto(comment);
    }

    public async Task<IEnumerable<CommentReadDto>> GetCommentsByTaskIdAsync(Guid taskId) {
        var comments = await _commentRepository.GetCommentsByTaskIdAsync(taskId);
        return comments.Select(MapToCommentReadDto);
    }

    public async Task<CommentReadDto> CreateCommentAsync(CommentCreateDto commentCreateDto,
        Guid userId) {
        var taskExists = await _workTaskRepository.GetTaskByIdAsync(commentCreateDto.TaskId);
        if (taskExists == null)
            throw new Exception($"Task with ID {commentCreateDto.TaskId} not found");

        var comment = new Comment {
            Id = Guid.NewGuid(),
            TaskId = commentCreateDto.TaskId,
            UserId = userId,
            Content = commentCreateDto.Content,
            CreatedDate = DateTime.UtcNow
        };

        await _commentRepository.AddCommentAsync(comment);

        return MapToCommentReadDto(comment);
    }

    public async Task<CommentReadDto> UpdateCommentAsync(Guid commentId,
        CommentUpdateDto commentUpdateDto) {
        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment == null)
            throw new Exception($"Comment with ID {commentId} not found");

        var updatedComment = commentUpdateDto.ToEntity(comment);
        await _commentRepository.UpdateCommentAsync(comment);

        return MapToCommentReadDto(updatedComment);
    }

    public async Task DeleteCommentAsync(Guid commentId) {
        await _commentRepository.DeleteCommentAsync(commentId);
    }


    private CommentReadDto MapToCommentReadDto(Comment comment) {
        return new CommentReadDto {
            Id = comment.Id,
            TaskId = comment.TaskId,
            UserId = comment.UserId,
            Content = comment.Content,
            CreatedDate = comment.CreatedDate,
            User = new UserReadDto {
                Id = comment.User.Id,
                Name = comment.User.Name,
                Email = comment.User.Email,
                CreatedDate = comment.User.CreatedDate
            }
        };
    }
}
