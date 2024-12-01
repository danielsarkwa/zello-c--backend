using Zello.Domain.Entities;

namespace Zello.Domain.RepositoryInterfaces;

public interface IProjectRepository : IBaseRepository<Project> {
    Task<Project?> GetProjectByIdWithDetailsAsync(Guid projectId);
    Task<List<Project>> GetProjectsByWorkspaceAsync(Guid? workspaceId);
    void Remove(Project project);
    Task<Project?> GetProjectWithMembersAsync(Guid projectId);
    Task AddProjectMemberAsync(ProjectMember projectMember);
    Task<int> GetMaxListPositionAsync(Guid projectId);
    Task AddListAsync(TaskList list);
    Task<bool> ExistsAsync(Guid projectId);
    Task<List<TaskList>> GetProjectListsAsync(Guid projectId);
    Task<ProjectMember?> GetProjectMemberAsync(Guid projectId, Guid workspaceMemberId);
}
