using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Zello.Api.Controllers;
using Zello.Application.Dtos;
using Zello.Application.Features.Tasks.Models;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Enums;

namespace Zello.Api.UnitTests;

public class TaskControllerTests {
    private readonly TaskController _controller;
    private readonly Mock<IWorkTaskService> _workTaskServiceMock;
    private readonly Guid _userId;
    private readonly AccessLevel _userAccess;

    public TaskControllerTests() {
        _workTaskServiceMock = new Mock<IWorkTaskService>();
        _controller = new TaskController(_workTaskServiceMock.Object);
        _userId = Guid.NewGuid();
        _userAccess = AccessLevel.Admin;
        SetupControllerContext();
    }

    private void SetupControllerContext() {
        var claims = new List<Claim> {
            new("UserId", _userId.ToString()),
            new("AccessLevel", _userAccess.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetTaskById_ExistingTask_ReturnsOk() {
        var taskId = Guid.NewGuid();
        var task = new TaskReadDto { Id = taskId, Name = "Test Task" };

        _workTaskServiceMock.Setup(x => x.GetTaskByIdAsync(taskId, _userId, _userAccess))
            .ReturnsAsync(task);

        var result = await _controller.GetTaskById(taskId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(task, okResult.Value);
    }

    [Fact]
    public async Task GetTaskById_UnauthorizedAccess_ReturnsForbid() {
        var taskId = Guid.NewGuid();
        _workTaskServiceMock.Setup(x => x.GetTaskByIdAsync(taskId, _userId, _userAccess))
            .ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.GetTaskById(taskId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateTask_ValidUpdate_ReturnsOk() {
        var taskId = Guid.NewGuid();
        var updateDto = new TaskUpdateDto {
            Name = "Updated Task",
            Description = "Updated Description",
            Status = CurrentTaskStatus.InProgress,
            Priority = Priority.High,
            Deadline = DateTime.UtcNow.AddDays(1)
        };
        var updatedTask = new TaskReadDto { Id = taskId, Name = updateDto.Name };

        _workTaskServiceMock.Setup(x => x.UpdateTaskAsync(taskId, updateDto, _userId, _userAccess))
            .ReturnsAsync(updatedTask);

        var result = await _controller.UpdateTask(taskId, updateDto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updatedTask, okResult.Value);
    }

    [Fact]
    public async Task MoveTask_ValidMove_ReturnsOk() {
        var taskId = Guid.NewGuid();
        var targetListId = Guid.NewGuid();
        var request = new MoveTaskRequest(targetListId);
        var movedTask = new TaskReadDto { Id = taskId, ListId = targetListId };

        _workTaskServiceMock.Setup(x => x.MoveTaskAsync(taskId, targetListId, _userId, _userAccess))
            .ReturnsAsync(movedTask);

        var result = await _controller.MoveTask(taskId, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(movedTask, okResult.Value);
    }

    [Fact]
    public async Task AssignUserToTask_ValidAssignment_ReturnsCreated() {
        var taskId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var request = new AssignUserRequest { UserId = assigneeId };
        var assignee = new TaskAssigneeReadDto { Id = Guid.NewGuid(), UserId = assigneeId };

        _workTaskServiceMock
            .Setup(x => x.AssignUserToTaskAsync(taskId, assigneeId, _userId, _userAccess))
            .ReturnsAsync(assignee);

        var result = await _controller.AssignUserToTask(taskId, request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(assignee, createdResult.Value);
    }

    [Fact]
    public async Task RemoveTaskAssignee_ExistingAssignment_ReturnsNoContent() {
        var taskId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();

        _workTaskServiceMock.Setup(x =>
                x.RemoveTaskAssigneeAsync(taskId, assigneeId, _userId, _userAccess))
            .Returns(Task.CompletedTask);

        var result = await _controller.RemoveTaskAssignee(taskId, assigneeId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AddTaskComment_ValidComment_ReturnsCreated() {
        var taskId = Guid.NewGuid();
        var request = new AddCommentRequest { Content = "Test comment" };
        var comment = new CommentReadDto {
            Id = Guid.NewGuid(),
            Content = request.Content,
            TaskId = taskId,
            UserId = _userId
        };

        _workTaskServiceMock.Setup(x =>
                x.AddTaskCommentAsync(taskId, request.Content, _userId, _userAccess))
            .ReturnsAsync(comment);

        var result = await _controller.AddTaskComment(taskId, request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(comment, createdResult.Value);
    }

    [Fact]
    public async Task GetTaskComments_ValidTask_ReturnsOk() {
        var taskId = Guid.NewGuid();
        var comments = new List<CommentReadDto> {
            new() { Id = Guid.NewGuid(), Content = "Test Comment" }
        };

        _workTaskServiceMock.Setup(x => x.GetTaskCommentsAsync(taskId, _userId, _userAccess))
            .ReturnsAsync(comments);

        var result = await _controller.GetTaskComments(taskId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(comments, okResult.Value);
    }
}
