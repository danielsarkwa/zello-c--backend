using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;
using Zello.Infrastructure.Data;

namespace Zello.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository {
    public UserRepository(ApplicationDbContext context) : base(context) {
    }

    public async Task<User?> GetUserByUsernameAsync(string username) {
        return await _dbSet
            .Include(u => u.WorkspaceMembers)
            .Include(u => u.AssignedTasks)
            .Include(u => u.Comments)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null) {
        return !await _dbSet.AnyAsync(u =>
            u.Username == username &&
            (!excludeUserId.HasValue || u.Id != excludeUserId.Value));
    }
}
