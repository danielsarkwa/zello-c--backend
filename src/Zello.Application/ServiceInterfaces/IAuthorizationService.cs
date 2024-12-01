using Zello.Domain.Entities.Api.User;

namespace Zello.Application.ServiceInterfaces;

public interface IAuthorizationService {
    Task<bool> AuthorizeProjectAccessAsync(
        Guid userId,
        Guid projectId,
        AccessLevel requiredAccessLevel
    );
    Task<bool> AuthorizeProjectMembershipAsync(Guid userId, Guid projectId);
    Task<bool> AuthorizeCommentAccessAsync(
      Guid userId,
      Guid commentId,
      AccessLevel? currentUserAccessLevel
    );
    Task<bool> AuthorizeWorkspaceMembershipAsync(Guid workspaceId, Guid userId);
    Task<bool> HasSufficientMembershipPermissionsAsync(Guid workspaceId, Guid userId, AccessLevel userAccess);
    Task<bool> CanManageMembersAsync(Guid userId, Guid projectId, AccessLevel userAccess);
}
