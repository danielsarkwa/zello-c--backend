using Zello.Domain.Entities;

namespace Zello.Domain.RepositoryInterfaces;

public interface IUserRepository : IBaseRepository<User> {
    Task<User?> GetUserByUsernameAsync(string username);
    Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null);
}
