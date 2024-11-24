using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Features.Projects;

public class CreateProjectMemberDto {
    public Guid WorkspaceMemberId { get; set; }
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Member;
}
