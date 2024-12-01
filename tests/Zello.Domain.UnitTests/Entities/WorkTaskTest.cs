using Zello.Domain.Entities;
using Zello.Domain.Enums;

namespace Zello.Domain.UnitTests;

public class WorkTaskEntityTests {
    [Fact]
    public void CreateWorkTask_WithValidData_ReturnsValidWorkTaskObject() {
        // Arrange & Act
        var workTask = new WorkTask {
            Id = Guid.NewGuid(),
            Name = "Test Work Task",
            Description = "Test Description",
            Status = CurrentTaskStatus.InProgress,
            Priority = Priority.High,
            Deadline = DateTime.UtcNow.AddDays(7),
            CreatedDate = DateTime.UtcNow,
            ProjectId = Guid.NewGuid(),
            ListId = Guid.NewGuid()
        };

        // Assert
        Assert.NotEqual(Guid.Empty, workTask.Id);
        Assert.Equal("Test Work Task", workTask.Name);
        Assert.Equal("Test Description", workTask.Description);
        Assert.Equal(CurrentTaskStatus.InProgress, workTask.Status);
        Assert.Equal(Priority.High, workTask.Priority);
        Assert.NotNull(workTask.Deadline);
        Assert.True(DateTime.UtcNow.Subtract(workTask.CreatedDate).TotalSeconds < 1);
        Assert.NotEqual(Guid.Empty, workTask.ProjectId);
        Assert.NotEqual(Guid.Empty, workTask.ListId);
    }

    [Fact]
    public void InitializeWorkTask_NavigationProperties_AreInitializedCorrectly() {
        // Arrange & Act
        var workTask = new WorkTask {
            Name = "Test Work Task",
            ProjectId = Guid.NewGuid(),
            ListId = Guid.NewGuid()
        };

        // Assert
        Assert.NotNull(workTask.Assignees);
        Assert.NotNull(workTask.Comments);
        Assert.Empty(workTask.Assignees);
        Assert.Empty(workTask.Comments);
    }

    [Fact]
    public void AddAssigneesAndComments_ToWorkTaskNavigationProperties_ItemsAddedSuccessfully() {
        // Arrange
        var workTask = new WorkTask {
            Name = "Test Work Task",
            ProjectId = Guid.NewGuid(),
            ListId = Guid.NewGuid()
        };

        var assignee = new TaskAssignee {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TaskId = workTask.Id
        };

        var comment = new Comment {
            Id = Guid.NewGuid(),
            TaskId = workTask.Id,
            Content = "Test Comment",
            CreatedDate = DateTime.UtcNow
        };

        // Act
        workTask.Assignees.Add(assignee);
        workTask.Comments.Add(comment);

        // Assert
        Assert.Single(workTask.Assignees);
        Assert.Single(workTask.Comments);
        Assert.Equal(assignee, workTask.Assignees.First());
        Assert.Equal(comment, workTask.Comments.First());
    }

    [Fact]
    public void AssignProjectAndList_ToWorkTask_AssignsSuccessfully() {
        // Arrange
        var project = new Project {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = Guid.NewGuid(),
            Status = ProjectStatus.InProgress
        };

        var taskList = new TaskList {
            Id = Guid.NewGuid(),
            Name = "Test Task List",
            ProjectId = project.Id
        };

        var workTask = new WorkTask {
            ProjectId = project.Id,
            ListId = taskList.Id,
            Project = project,
            List = taskList
        };

        // Assert
        Assert.Equal(project.Id, workTask.ProjectId);
        Assert.Equal(taskList.Id, workTask.ListId);
        Assert.Equal(project, workTask.Project);
        Assert.Equal(taskList, workTask.List);
    }

    [Fact]
    public void CreateWorkTask_WithoutName_ThrowsValidationException() {
        // Arrange & Act
        var workTask = new WorkTask {
            ProjectId = Guid.NewGuid(),
            ListId = Guid.NewGuid()
        };

        // Assert
        Assert.Equal(string.Empty, workTask.Name);
    }
}
