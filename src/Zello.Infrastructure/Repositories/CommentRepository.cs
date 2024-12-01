using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;
using Zello.Infrastructure.Data;

namespace Zello.Infrastructure.Repositories;

public class CommentRepository : BaseRepository<Comment>, ICommentRepository {

    public CommentRepository(ApplicationDbContext context) : base(context) {
    }

    public async Task<Comment?> GetCommentByIdAsync(Guid commentId) {
        return await _dbSet
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }

    public async Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(Guid taskId) {
        return await _context.Comments
            .Where(c => c.TaskId == taskId)
            .Include(c => c.User)
            .ToListAsync();
    }

    public async Task<Comment> AddCommentAsync(Comment comment) {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment> UpdateCommentAsync(Comment comment) {
        var existingComment = await _context.Comments.FindAsync(comment.Id);
        if (existingComment == null) {
            throw new Exception($"Comment with ID {comment.Id} not found");
        }

        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId) {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) {
            return false;
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }
}
