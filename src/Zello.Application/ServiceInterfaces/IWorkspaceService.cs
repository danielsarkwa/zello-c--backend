using Zello.Application.Dtos;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.ServiceInterfaces;

public interface IWorkspaceService {
    Task<WorkspaceReadDto> CreateWorkspaceAsync(WorkspaceCreateDto createWorkspace, Guid userId);
    Task<List<WorkspaceReadDto>> GetAllWorkspacesAsync(Guid userId, AccessLevel? userAccess);

    Task<WorkspaceReadDto> GetWorkspaceByIdAsync(Guid workspaceId, Guid userId,
        AccessLevel? userAccess);

    Task<WorkspaceReadDto> UpdateWorkspaceAsync(Guid workspaceId,
        WorkspaceUpdateDto workspaceUpdateDto, Guid userId, AccessLevel? userAccess);

    Task DeleteWorkspaceAsync(Guid workspaceId, Guid userId, AccessLevel? userAccess);

    Task<WorkspaceMemberReadDto> AddWorkspaceMemberAsync(Guid workspaceId,
        WorkspaceMemberCreateDto createMember, Guid userId, AccessLevel? userAccess);

    Task<List<WorkspaceMemberReadDto>> GetWorkspaceMembersAsync(Guid workspaceId, Guid userId,
        AccessLevel? userAccess);

    Task<WorkspaceMemberReadDto> UpdateMemberAccessAsync(Guid memberId, WorkspaceMemberUpdateDto updateDto, Guid userId, AccessLevel? userAccess);
}
