using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Domain.UnitTests;

public class WorkspaceMemberEntityTests {
    [Fact]
    public void CreateWorkspaceMember_WithValidData_ReturnsValidWorkspaceMemberObject() {
        // Arrange & Act
        var workspaceMember = new WorkspaceMember {
            Id = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AccessLevel = AccessLevel.Admin,
            CreatedDate = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, workspaceMember.Id);
        Assert.NotEqual(Guid.Empty, workspaceMember.WorkspaceId);
        Assert.NotEqual(Guid.Empty, workspaceMember.UserId);
        Assert.Equal(AccessLevel.Admin, workspaceMember.AccessLevel);
        Assert.True(DateTime.UtcNow.Subtract(workspaceMember.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void CreateWorkspaceMember_DefaultAccessLevel_IsMember() {
        // Arrange & Act
        var workspaceMember = new WorkspaceMember {
            WorkspaceId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        // Assert
        Assert.Equal(AccessLevel.Member, workspaceMember.AccessLevel);
    }

    [Fact]
    public void CreateWorkspaceMember_DefaultCreatedDate_IsSetToUtcNow() {
        // Arrange & Act
        var workspaceMember = new WorkspaceMember {
            WorkspaceId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        // Assert
        Assert.True(DateTime.UtcNow.Subtract(workspaceMember.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void AssignWorkspaceAndUser_ToWorkspaceMember_AssignsSuccessfully() {
        // Arrange
        var workspace = new Workspace {
            Id = Guid.NewGuid(),
            Name = "Test Workspace",
            OwnerId = Guid.NewGuid()
        };

        var user = new User {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };

        var workspaceMember = new WorkspaceMember {
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Workspace = workspace,
            User = user
        };

        // Assert
        Assert.Equal(workspace.Id, workspaceMember.WorkspaceId);
        Assert.Equal(user.Id, workspaceMember.UserId);
        Assert.Equal(workspace, workspaceMember.Workspace);
        Assert.Equal(user, workspaceMember.User);
    }
}
