using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Features.Workspaces;

public class CreateWorkspaceMemberDto {
    public Guid UserId { get; set; }
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Member; // Default to Member
}
