using Zello.Domain.Entities.Api.User;
namespace Zello.Application.ServiceInterfaces.ExceptionInterfaces;

public interface IWorkspaceValidationService {
    Task ValidateWorkspaceAccess(Guid workspaceId, Guid userId, AccessLevel? userAccess);
    Task ValidateManagePermissions(Guid workspaceId, Guid userId, AccessLevel? userAccess);
    Task ValidateAccessLevelAssignment(Guid workspaceId, Guid userId, AccessLevel newLevel, AccessLevel? adminAccess);
    Task EnsureWorkspaceExists(Guid workspaceId);
    Task EnsureUserExists(Guid userId);
    Task EnsureNotExistingMember(Guid workspaceId, Guid userId);
}
