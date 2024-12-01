using Zello.Domain.Entities;

namespace Zello.Domain.RepositoryInterfaces;

public interface IWorkspaceRepository : IBaseRepository<Workspace> {
    Task<Workspace> GetWorkspaceWithDetailsAsync(Guid workspaceId);
    Task<List<Workspace>> GetAllWorkspacesWithDetailsAsync();
    Task<WorkspaceMember> AddWorkspaceMemberAsync(WorkspaceMember member);
    Task<List<WorkspaceMember>> GetWorkspaceMembersAsync(Guid workspaceId);

    Task<WorkspaceMember?> GetMemberByIdAsync(Guid memberId);
    Task<WorkspaceMember> UpdateMemberAsync(WorkspaceMember member);
}
