using Zello.Domain.Entities;

namespace Zello.Infrastructure.Interfaces;

public interface IProjectRepository : IRepository<Project> {
    Task<IEnumerable<Project>> GetProjectsByWorkspaceIdAsync(Guid workspaceId);
    Task<IEnumerable<Project>> GetProjectsByUserIdAsync(Guid userId);
}
