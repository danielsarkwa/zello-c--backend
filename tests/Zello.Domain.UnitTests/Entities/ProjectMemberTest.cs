using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Enums;

namespace Zello.Domain.UnitTests;

public class ProjectMemberEntityTests {
    [Fact]
    public void CreateProjectMember_WithValidData_ReturnsValidProjectMemberObject() {
        // Arrange & Act
        var projectMember = new ProjectMember {
            Id = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            WorkspaceMemberId = Guid.NewGuid(),
            AccessLevel = AccessLevel.Admin,
            CreatedDate = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, projectMember.Id);
        Assert.NotEqual(Guid.Empty, projectMember.ProjectId);
        Assert.NotEqual(Guid.Empty, projectMember.WorkspaceMemberId);
        Assert.Equal(AccessLevel.Admin, projectMember.AccessLevel);
        Assert.True(DateTime.UtcNow.Subtract(projectMember.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void CreateProjectMember_DefaultAccessLevel_IsMember() {
        // Arrange & Act
        var projectMember = new ProjectMember {
            ProjectId = Guid.NewGuid(),
            WorkspaceMemberId = Guid.NewGuid()
        };

        // Assert
        Assert.Equal(AccessLevel.Member, projectMember.AccessLevel);
    }

    [Fact]
    public void CreateProjectMember_DefaultCreatedDate_IsSetToUtcNow() {
        // Arrange & Act
        var projectMember = new ProjectMember {
            ProjectId = Guid.NewGuid(),
            WorkspaceMemberId = Guid.NewGuid()
        };

        // Assert
        Assert.True(DateTime.UtcNow.Subtract(projectMember.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void AssignProjectAndWorkspaceMember_ToProjectMember_AssignsSuccessfully() {
        // Arrange
        var project = new Project {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = Guid.NewGuid(),
            Status = ProjectStatus.InProgress
        };

        var workspaceMember = new WorkspaceMember {
            Id = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AccessLevel = AccessLevel.Member
        };

        var projectMember = new ProjectMember {
            ProjectId = project.Id,
            WorkspaceMemberId = workspaceMember.Id,
            Project = project,
            WorkspaceMember = workspaceMember
        };

        // Assert
        Assert.Equal(project.Id, projectMember.ProjectId);
        Assert.Equal(workspaceMember.Id, projectMember.WorkspaceMemberId);
        Assert.Equal(project, projectMember.Project);
        Assert.Equal(workspaceMember, projectMember.WorkspaceMember);
    }
}
