using Zello.Domain.Entities;

namespace Zello.Domain.RepositoryInterfaces;

public interface ICommentRepository : IBaseRepository<Comment> {
    Task<Comment?> GetCommentByIdAsync(Guid commentId);
    Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(Guid taskId);
    Task<Comment> AddCommentAsync(Comment comment);
    Task<Comment> UpdateCommentAsync(Comment comment);
    Task<bool> DeleteCommentAsync(Guid commentId);
}
