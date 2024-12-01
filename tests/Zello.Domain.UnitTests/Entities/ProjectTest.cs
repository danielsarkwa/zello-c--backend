using System;
using System.Collections.Generic;
using Xunit;
using Zello.Domain.Entities;
using Zello.Domain.Enums;

namespace Zello.Domain.UnitTests;

public class ProjectEntityTests {
    [Fact]
    public void CreateProject_WithValidData_ReturnsValidProjectObject() {
        // Arrange & Act
        var project = new Project {
            Id = Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            Status = ProjectStatus.InProgress,
            CreatedDate = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, project.Id);
        Assert.NotEqual(Guid.Empty, project.WorkspaceId);
        Assert.Equal("Test Project", project.Name);
        Assert.Equal("Test Description", project.Description);
        Assert.NotNull(project.StartDate);
        Assert.NotNull(project.EndDate);
        Assert.Equal(ProjectStatus.InProgress, project.Status);
        Assert.True(DateTime.UtcNow.Subtract(project.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void InitializeProject_NavigationProperties_AreInitializedCorrectly() {
        // Arrange & Act
        var project = new Project {
            Name = "Test Project",
            WorkspaceId = Guid.NewGuid()
        };

        // Assert
        Assert.NotNull(project.Members);
        Assert.NotNull(project.Lists);
        Assert.Empty(project.Members);
        Assert.Empty(project.Lists);
    }

    [Fact]
    public void AddItems_ToProjectNavigationProperties_ItemsAddedSuccessfully() {
        // Arrange
        var project = new Project {
            Name = "Test Project",
            WorkspaceId = Guid.NewGuid()
        };

        var projectMember = new ProjectMember();
        var taskList = new TaskList();

        // Act
        project.Members.Add(projectMember);
        project.Lists.Add(taskList);

        // Assert
        Assert.Single(project.Members);
        Assert.Single(project.Lists);
    }

    [Fact]
    public void CreateProject_DefaultCreatedDate_IsSetToUtcNow() {
        // Arrange & Act
        var project = new Project {
            Name = "Test Project",
            WorkspaceId = Guid.NewGuid()
        };

        // Assert
        Assert.True(DateTime.UtcNow.Subtract(project.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void CreateProject_WithoutName_ReturnsEmptyName() {
        // Arrange & Act
        var project = new Project {
            WorkspaceId = Guid.NewGuid()
        };

        // Assert
        Assert.Equal(string.Empty, project.Name);
    }
}
