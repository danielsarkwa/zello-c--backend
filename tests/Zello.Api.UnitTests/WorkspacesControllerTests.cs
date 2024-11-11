using Microsoft.AspNetCore.Mvc;
using Xunit;
using Zello.Api.Controllers;

namespace Zello.Api.UnitTests;

public class WorkspacesControllerTests {
    private readonly WorkspacesController _controller;

    public WorkspacesControllerTests() {
        _controller = new WorkspacesController();
    }

    [Fact]
    public void CreateWorkspace_Returns200OK() {
        // Arrange
        var workspace = new { Name = "Test Workspace" };

        // Act
        var result = _controller.CreateWorkspace(workspace);

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("Workspace created", contentResult.Content);
    }

    [Fact]
    public void GetWorkspace_Returns200OK() {
        // Act
        var result = _controller.GetWorkspace("workspace-id");

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("Workspace retrieved", contentResult.Content);
    }

    [Fact]
    public void UpdateWorkspace_Returns200OK() {
        // Arrange
        var workspace = new { Name = "Updated Workspace" };

        // Act
        var result = _controller.UpdateWorkspace("workspace-id", workspace);

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("Workspace updated", contentResult.Content);
    }

    [Fact]
    public void DeleteWorkspace_Returns200OK() {
        // Act
        var result = _controller.DeleteWorkspace("workspace-id");

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("Workspace deleted", contentResult.Content);
    }

    [Fact]
    public void GetWorkspaceProjects_Returns200OK() {
        // Act
        var result = _controller.GetWorkspaceProjects("workspace-id");

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("Workspace projects retrieved", contentResult.Content);
    }

    [Fact]
    public void GetWorkspaceLabels_Returns200OK() {
        // Act
        var result = _controller.GetWorkspaceLabels("workspace-id");

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("Workspace labels retrieved", contentResult.Content);
    }

    [Fact]
    public void AddWorkspaceMember_Returns200OK() {
        // Arrange
        var member = new { UserId = "user-id", Role = "member" };

        // Act
        var result = _controller.AddWorkspaceMember("workspace-id", member);

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("Workspace member added", contentResult.Content);
    }

    [Fact]
    public void CreateCustomRole_Returns200OK() {
        // Arrange
        var customRole = new { Name = "Custom Role" };

        // Act
        var result = _controller.CreateCustomRole("workspace-id", customRole);

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("Custom role created", contentResult.Content);
    }
}
