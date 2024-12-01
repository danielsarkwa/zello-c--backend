using Zello.Domain.Entities;

namespace Zello.Domain.UnitTests;

public class WorkspaceEntityTests {
    [Fact]
    public void CreateWorkspace_WithValidData_ReturnsValidWorkspaceObject() {
        // Arrange & Act
        var workspace = new Workspace {
            Id = Guid.NewGuid(),
            Name = "Test Workspace",
            OwnerId = Guid.NewGuid(),
            CreatedDate = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, workspace.Id);
        Assert.Equal("Test Workspace", workspace.Name);
        Assert.NotEqual(Guid.Empty, workspace.OwnerId);
        Assert.True(DateTime.UtcNow.Subtract(workspace.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void InitializeWorkspace_NavigationProperties_AreInitializedCorrectly() {
        // Arrange & Act
        var workspace = new Workspace {
            Name = "Test Workspace",
            OwnerId = Guid.NewGuid()
        };

        // Assert
        Assert.NotNull(workspace.Projects);
        Assert.NotNull(workspace.Members);
        Assert.Empty(workspace.Projects);
        Assert.Empty(workspace.Members);
    }

    [Fact]
    public void AddItems_ToWorkspaceNavigationProperties_ItemsAddedSuccessfully() {
        // Arrange
        var workspace = new Workspace {
            Name = "Test Workspace",
            OwnerId = Guid.NewGuid()
        };

        var project = new Project();
        var workspaceMember = new WorkspaceMember();

        // Act
        workspace.Projects.Add(project);
        workspace.Members.Add(workspaceMember);

        // Assert
        Assert.Single(workspace.Projects);
        Assert.Single(workspace.Members);
    }

    [Fact]
    public void CreateWorkspace_DefaultCreatedDate_IsSetToUtcNow() {
        // Arrange & Act
        var workspace = new Workspace {
            Name = "Test Workspace",
            OwnerId = Guid.NewGuid()
        };

        // Assert
        Assert.True(DateTime.UtcNow.Subtract(workspace.CreatedDate).TotalSeconds < 1);
    }
}
