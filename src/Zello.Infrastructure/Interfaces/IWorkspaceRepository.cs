using Zello.Domain.Entities;

namespace Zello.Infrastructure.Interfaces;

public interface IWorkspaceRepository : IRepository<Workspace> {
    Task<IEnumerable<Workspace>> GetWorkspacesByUserIdAsync(Guid userId);
    Task<bool> IsUserWorkspaceOwnerAsync(Guid userId, Guid workspaceId);
}
