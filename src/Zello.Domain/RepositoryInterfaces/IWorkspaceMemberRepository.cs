using Zello.Domain.Entities;

namespace Zello.Domain.RepositoryInterfaces;

public interface IWorkspaceMemberRepository : IBaseRepository<WorkspaceMember> {
    Task<WorkspaceMember?> GetWorkspaceMemberAsync(Guid workspaceId, Guid userId);
    Task<IEnumerable<WorkspaceMember>> GetMembersByWorkspaceIdAsync(Guid workspaceId);
}
