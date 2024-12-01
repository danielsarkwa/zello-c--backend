using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.ServiceInterfaces;

public interface IProjectService {
    Task<ProjectReadDto> CreateProjectAsync(ProjectCreateDto projectDto, Guid userId);
    Task<ProjectReadDto> GetProjectByIdAsync(Guid projectId);
    Task<List<Project>> GetAllProjectsAsync(Guid? workspaceId);
    Task<ProjectReadDto> UpdateProjectAsync(Guid projectId, ProjectUpdateDto updatedProject);
    Task DeleteProjectAsync(Guid projectId);

    Task<ProjectMember> AddProjectMemberAsync(ProjectMemberCreateDto createMember,
        Guid currentUserId);

    Task<TaskList> CreateListAsync(Guid projectId, ListCreateDto listDto);
    Task<List<TaskList>> GetProjectListsAsync(Guid projectId);

    Task<ProjectMember> UpdateMemberAccessAsync(Guid memberId, AccessLevel newAccessLevel,
        Guid currentUserId, AccessLevel userAccess);
}
