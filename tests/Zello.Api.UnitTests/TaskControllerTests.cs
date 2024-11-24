using Microsoft.AspNetCore.Mvc;
using Zello.Api.Controllers;
using Zello.Domain.Enums;
using Zello.Application.Features.Tasks.Models;

namespace Zello.Api.UnitTests;

public class TaskControllerTests {
    private readonly TaskController _controller;

    public TaskControllerTests() {
        _controller = new TaskController();
    }

    [Fact]
    public void GetTaskById_NonexistentId_ReturnsNotFound() {
        // Arrange
        var taskId = Guid.NewGuid();

        // Act
        var result = _controller.GetTaskById(taskId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(taskId.ToString(), notFoundResult.Value.ToString());
    }

    [Fact]
    public void UpdateTask_NonexistentId_ReturnsNotFound() {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = new UpdateTaskRequest {
            Name = "Updated Task",
            Description = "Updated Description",
            Status = CurrentTaskStatus.InProgress,
            Priority = Priority.Medium,
            Deadline = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = _controller.UpdateTask(taskId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(taskId.ToString(), notFoundResult.Value.ToString());
    }

    [Fact]
    public void DeleteTask_NonexistentId_ReturnsNotFound() {
        // Arrange
        var taskId = Guid.NewGuid();

        // Act
        var result = _controller.DeleteTask(taskId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(taskId.ToString(), notFoundResult.Value.ToString());
    }

    [Fact]
    public void MoveTask_NonexistentTask_ReturnsNotFound() {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = new MoveTaskRequest(TargetListId: Guid.NewGuid());

        // Act
        var result = _controller.MoveTask(taskId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(taskId.ToString(), notFoundResult.Value.ToString());
    }

    [Fact]
    public void GetTaskComments_NonexistentTask_ReturnsNotFound() {
        // Arrange
        var taskId = Guid.NewGuid();

        // Act
        var result = _controller.GetTaskComments(taskId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(taskId.ToString(), notFoundResult.Value.ToString());
    }

    [Fact]
    public void AddTaskComment_NonexistentTask_ReturnsNotFound() {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = new AddCommentRequest(Content: "Test Comment");

        // Act
        var result = _controller.AddTaskComment(taskId, request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(taskId.ToString(), notFoundResult.Value.ToString());
    }

    [Fact]
    public void AddTaskLabels_NonexistentTask_ReturnsNotFound() {
        // Arrange
        var taskId = Guid.NewGuid();
        var labels = new List<LabelDto>();

        // Act
        var result = _controller.AddTaskLabels(taskId, labels);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(taskId.ToString(), notFoundResult.Value.ToString());
    }

    [Fact]
    public void AddTaskAssignees_NonexistentTask_ReturnsNotFound() {
        // Arrange
        var taskId = Guid.NewGuid();
        var userIds = new List<Guid> { Guid.NewGuid() };

        // Act
        var result = _controller.AddTaskAssignees(taskId, userIds);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(taskId.ToString(), notFoundResult.Value.ToString());
    }
}
