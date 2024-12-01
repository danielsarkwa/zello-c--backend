using Zello.Application.Dtos;

namespace Zello.Application.ServiceInterfaces;

public interface ICommentService {
    Task<CommentReadDto> GetCommentByIdAsync(Guid commentId);
    Task<IEnumerable<CommentReadDto>> GetCommentsByTaskIdAsync(Guid taskId);
    Task<CommentReadDto> CreateCommentAsync(CommentCreateDto commentCreateDto, Guid userId);
    Task<CommentReadDto> UpdateCommentAsync(Guid commentId, CommentUpdateDto request);
    Task<TaskProjectDetailsDto> GetTaskProjectDetailsAsync(Guid taskId);
    Task DeleteCommentAsync(Guid commentId);
}
