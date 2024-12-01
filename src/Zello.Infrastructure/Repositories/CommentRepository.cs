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
            .AsNoTracking() // Add this to prevent tracking conflicts
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }

    public async Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(Guid taskId) {
        return await _context.Comments
            .Where(c => c.TaskId == taskId)
            .Include(c => c.User)
            .AsNoTracking() // Add this to prevent tracking conflicts
            .ToListAsync();
    }

    public async Task<Comment> AddCommentAsync(Comment comment) {
        // Detach any existing entities with the same ID to prevent tracking conflicts
        var local = _context.Comments.Local.FirstOrDefault(c => c.Id == comment.Id);
        if (local != null) {
            _context.Entry(local).State = EntityState.Detached;
        }

        var entry = await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();

        // Refresh the comment to include any generated values
        await entry.Reference(c => c.User).LoadAsync();

        return entry.Entity;
    }

    public async Task<Comment> UpdateCommentAsync(Comment comment) {
        // First detach any existing entity with the same ID
        var local = _context.Comments.Local.FirstOrDefault(c => c.Id == comment.Id);
        if (local != null) {
            _context.Entry(local).State = EntityState.Detached;
        }

        // Then attach and mark as modified
        _context.Entry(comment).State = EntityState.Modified;

        try {
            await _context.SaveChangesAsync();
            return comment;
        } catch (DbUpdateConcurrencyException) {
            if (!await CommentExists(comment.Id))
                throw new KeyNotFoundException($"Comment with ID {comment.Id} not found");
            throw;
        }
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

    private async Task<bool> CommentExists(Guid id) {
        return await _context.Comments.AnyAsync(c => c.Id == id);
    }
}
