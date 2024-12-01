using System;
using Xunit;
using Zello.Domain.Entities;

namespace Zello.Domain.UnitTests;

public class TaskAssigneeEntityTests {
    [Fact]
    public void CreateTaskAssignee_WithValidData_ReturnsValidTaskAssigneeObject() {
        // Arrange & Act
        var taskAssignee = new TaskAssignee {
            Id = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AssignedDate = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, taskAssignee.Id);
        Assert.NotEqual(Guid.Empty, taskAssignee.TaskId);
        Assert.NotEqual(Guid.Empty, taskAssignee.UserId);
        Assert.True(DateTime.UtcNow.Subtract(taskAssignee.AssignedDate).TotalSeconds < 1);
    }

    [Fact]
    public void CreateTaskAssignee_DefaultAssignedDate_IsSetToUtcNow() {
        // Arrange & Act
        var taskAssignee = new TaskAssignee {
            TaskId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        // Assert
        Assert.True(DateTime.UtcNow.Subtract(taskAssignee.AssignedDate).TotalSeconds < 1);
    }

    [Fact]
    public void AssignTaskAndUser_ToTaskAssignee_AssignsSuccessfully() {
        // Arrange
        var workTask = new WorkTask {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            ProjectId = Guid.NewGuid(),
            ListId = Guid.NewGuid()
        };

        var user = new User {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };

        var taskAssignee = new TaskAssignee {
            TaskId = workTask.Id,
            UserId = user.Id,
            Task = workTask,
            User = user
        };

        // Assert
        Assert.Equal(workTask.Id, taskAssignee.TaskId);
        Assert.Equal(user.Id, taskAssignee.UserId);
        Assert.Equal(workTask, taskAssignee.Task);
        Assert.Equal(user, taskAssignee.User);
    }
}
