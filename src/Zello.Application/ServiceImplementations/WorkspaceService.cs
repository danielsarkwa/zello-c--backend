using Zello.Application.Dtos;
using Zello.Application.Exceptions;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.RepositoryInterfaces;
using Zello.Application.ServiceInterfaces;
using Zello.Application.ServiceInterfaces.ExceptionInterfaces;

namespace Zello.Application.ServiceImplementations;

public class WorkspaceService : IWorkspaceService {
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceValidationService _validationService;

    public WorkspaceService(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceValidationService validationService) {
        _workspaceRepository = workspaceRepository;
        _validationService = validationService;
    }

    public async Task<WorkspaceReadDto> CreateWorkspaceAsync(WorkspaceCreateDto createWorkspace,
        Guid userId) {
        await _validationService.EnsureUserExists(userId);

        var workspace = createWorkspace.ToEntity(userId);
        var savedWorkspace = await _workspaceRepository.AddAsync(workspace);
        return WorkspaceReadDto.FromEntity(savedWorkspace);
    }

    public async Task<List<WorkspaceReadDto>> GetAllWorkspacesAsync(Guid userId,
        AccessLevel? userAccess) {
        await _validationService.EnsureUserExists(userId);
        var workspaces = await _workspaceRepository.GetAllWorkspacesWithDetailsAsync();

        if (userAccess != AccessLevel.Admin) {
            workspaces = workspaces.Where(w => w.Members.Any(m => m.UserId == userId)).ToList();
        }

        return workspaces.Select(WorkspaceReadDto.FromEntity).ToList();
    }

    public async Task<WorkspaceReadDto> GetWorkspaceByIdAsync(Guid workspaceId, Guid userId,
        AccessLevel? userAccess) {
        var workspace = await _workspaceRepository.GetWorkspaceWithDetailsAsync(workspaceId);
        await _validationService.EnsureWorkspaceExists(workspaceId);
        await _validationService.ValidateWorkspaceAccess(workspaceId, userId, userAccess);

        return WorkspaceReadDto.FromEntity(workspace!);
    }

    public async Task<WorkspaceReadDto> UpdateWorkspaceAsync(Guid workspaceId,
        WorkspaceUpdateDto updateDto, Guid userId, AccessLevel? userAccess) {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        await _validationService.EnsureWorkspaceExists(workspaceId);
        await _validationService.ValidateManagePermissions(workspaceId, userId, userAccess);

        var updatedWorkspace = updateDto.ToEntity(workspace!);
        await _workspaceRepository.UpdateAsync(updatedWorkspace);
        return WorkspaceReadDto.FromEntity(updatedWorkspace);
    }

    public async Task DeleteWorkspaceAsync(Guid workspaceId, Guid userId, AccessLevel? userAccess) {
        var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
        await _validationService.EnsureWorkspaceExists(workspaceId);
        await _validationService.ValidateManagePermissions(workspaceId, userId, userAccess);

        await _workspaceRepository.DeleteAsync(workspace!);
    }

    public async Task<WorkspaceMemberReadDto> AddWorkspaceMemberAsync(
        Guid workspaceId,
        WorkspaceMemberCreateDto createMember,
        Guid userId,
        AccessLevel? userAccess) {
        await _validationService.EnsureWorkspaceExists(workspaceId);
        await _validationService.ValidateManagePermissions(workspaceId, userId, userAccess);
        await _validationService.EnsureUserExists(createMember.UserId);
        await _validationService.EnsureNotExistingMember(workspaceId, createMember.UserId);
        await _validationService.ValidateAccessLevelAssignment(workspaceId, userId,
            createMember.AccessLevel, userAccess);

        var member = createMember.ToEntity(workspaceId);
        var savedMember = await _workspaceRepository.AddWorkspaceMemberAsync(member);
        return WorkspaceMemberReadDto.FromEntity(savedMember);
    }

    public async Task<List<WorkspaceMemberReadDto>> GetWorkspaceMembersAsync(Guid workspaceId,
        Guid userId, AccessLevel? userAccess) {
        await _validationService.EnsureWorkspaceExists(workspaceId);
        await _validationService.ValidateWorkspaceAccess(workspaceId, userId, userAccess);

        var members = await _workspaceRepository.GetWorkspaceMembersAsync(workspaceId);
        return members.Select(WorkspaceMemberReadDto.FromEntity).ToList();
    }

    public async Task<WorkspaceMemberReadDto> UpdateMemberAccessAsync(
        Guid memberId,
        WorkspaceMemberUpdateDto updateDto,
        Guid userId,
        AccessLevel? userAccess) {
        var member = await _workspaceRepository.GetMemberByIdAsync(memberId);
        if (member == null) throw new WorkspaceMemberNotFoundException();

        await _validationService.ValidateManagePermissions(member.WorkspaceId, userId, userAccess);
        await _validationService.ValidateAccessLevelAssignment(member.WorkspaceId, userId,
            updateDto.Role, userAccess);

        var updatedMember = updateDto.ToEntity(member);
        await _workspaceRepository.UpdateMemberAsync(updatedMember);
        return WorkspaceMemberReadDto.FromEntity(updatedMember);
    }
}
