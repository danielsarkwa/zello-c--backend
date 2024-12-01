using Zello.Domain.Entities;
using Zello.Domain.Enums;

namespace Zello.Domain.UnitTests;

public class TaskListEntityTests {
    [Fact]
    public void CreateTaskList_WithValidData_ReturnsValidTaskListObject() {
        // Arrange & Act
        var taskList = new TaskList {
            Id = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Name = "Test Task List",
            Position = 1,
            CreatedDate = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, taskList.Id);
        Assert.NotEqual(Guid.Empty, taskList.ProjectId);
        Assert.Equal("Test Task List", taskList.Name);
        Assert.Equal(1, taskList.Position);
        Assert.True(DateTime.UtcNow.Subtract(taskList.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void InitializeTaskList_NavigationProperties_AreInitializedCorrectly() {
        // Arrange & Act
        var taskList = new TaskList {
            Name = "Test Task List",
            ProjectId = Guid.NewGuid()
        };

        // Assert
        Assert.NotNull(taskList.Tasks);
        Assert.Empty(taskList.Tasks);
    }

    [Fact]
    public void AddTasks_ToTaskListNavigationProperty_TasksAddedSuccessfully() {
        // Arrange
        var taskList = new TaskList {
            Name = "Test Task List",
            ProjectId = Guid.NewGuid()
        };

        var workTask = new WorkTask {
            Id = Guid.NewGuid(),
            ListId = taskList.Id,
            Name = "Test Work Task",
            Description = "Test Description",
            Status = CurrentTaskStatus.NotStarted
        };

        // Act
        taskList.Tasks.Add(workTask);

        // Assert
        Assert.Single(taskList.Tasks);
        Assert.Equal(workTask, taskList.Tasks.First());
    }

    [Fact]
    public void AssignProject_ToTaskList_AssignsSuccessfully() {
        // Arrange
        var project = new Project {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = Guid.NewGuid(),
            Status = ProjectStatus.InProgress
        };

        var taskList = new TaskList {
            ProjectId = project.Id,
            Project = project
        };

        // Assert
        Assert.Equal(project.Id, taskList.ProjectId);
        Assert.Equal(project, taskList.Project);
    }
}
