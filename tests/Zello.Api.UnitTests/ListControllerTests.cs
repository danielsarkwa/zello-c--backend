using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Zello.Api.Controllers;
using Zello.Application.Dtos;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Enums;

namespace Zello.Api.UnitTests;

public class ListControllerTests {
    private readonly ListController _controller;
    private readonly Mock<ITaskListService> _taskListServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IProjectService> _projectServiceMock;
    private readonly Mock<IAuthorizationService> _authServiceMock;
    private readonly Guid _userId;
    private readonly AccessLevel _userAccess;

    public ListControllerTests() {
        _taskListServiceMock = new Mock<ITaskListService>();
        _userServiceMock = new Mock<IUserService>();
        _projectServiceMock = new Mock<IProjectService>();
        _authServiceMock = new Mock<IAuthorizationService>();

        _controller = new ListController(
            _taskListServiceMock.Object,
            _userServiceMock.Object,
            _projectServiceMock.Object,
            _authServiceMock.Object
        );

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
    public async Task GetListById_ExistingList_ReturnsOkResult() {
        var listId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var list = new ListReadDto { Id = listId, ProjectId = projectId };

        _taskListServiceMock.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(list);
        _authServiceMock.Setup(x =>
                x.AuthorizeProjectAccessAsync(_userId, projectId, AccessLevel.Member))
            .ReturnsAsync(true);

        var result = await _controller.GetListById(listId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(list, okResult.Value);
    }

    [Fact]
    public async Task GetListById_UnauthorizedAccess_ReturnsForbid() {
        var listId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var list = new ListReadDto { Id = listId, ProjectId = projectId };

        _taskListServiceMock.Setup(x => x.GetByIdAsync(listId)).ReturnsAsync(list);
        _authServiceMock.Setup(x =>
                x.AuthorizeProjectAccessAsync(_userId, projectId, AccessLevel.Member))
            .ReturnsAsync(false);

        var result = await _controller.GetListById(listId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task CreateTask_ValidInput_ReturnsCreatedResult() {
        var listId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var createDto = new TaskCreateDto {
            Name = "Test Task",
            ProjectId = projectId,
            ListId = listId,
            Status = CurrentTaskStatus.NotStarted,
            Priority = Priority.Medium
        };
        var task = new TaskReadDto { Id = Guid.NewGuid(), Name = createDto.Name };
        var tasks = new List<TaskReadDto> { new() { ProjectId = projectId } };

        _taskListServiceMock.Setup(x => x.CreateTaskAsync(listId, createDto, _userId))
            .ReturnsAsync(task);
        _taskListServiceMock.Setup(x => x.GetListTasksAsync(listId))
            .ReturnsAsync(tasks);
        _authServiceMock.Setup(x => x.AuthorizeProjectMembershipAsync(_userId, projectId))
            .ReturnsAsync(true);

        var result = await _controller.CreateTask(listId, createDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(task, createdResult.Value);
    }

    [Fact]
    public async Task UpdateListPosition_UnauthorizedAccess_ThrowsUnauthorizedException() {
        var listId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var updateDto = new ListUpdateDto { Position = 1 };

        _taskListServiceMock.Setup(x => x.GetByIdAsync(listId))
            .ReturnsAsync(new ListReadDto { Id = listId, ProjectId = projectId });
        _authServiceMock.Setup(x =>
                x.AuthorizeProjectAccessAsync(_userId, projectId, AccessLevel.Member))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _controller.UpdateListPosition(listId, updateDto));
    }

    [Fact]
    public async Task GetListTasks_ExistingList_ReturnsOkResult() {
        var listId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var tasks = new List<TaskReadDto> {
            new() {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = "Test Task"
            }
        };

        _taskListServiceMock.Setup(x => x.GetListTasksAsync(listId))
            .ReturnsAsync(tasks);
        _authServiceMock.Setup(x => x.AuthorizeProjectMembershipAsync(_userId, projectId))
            .ReturnsAsync(true);

        var result = await _controller.GetListTasks(listId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTasks = Assert.IsAssignableFrom<IEnumerable<TaskReadDto>>(okResult.Value);
        Assert.Equal(tasks, returnedTasks);
    }

    [Fact]
    public async Task GetListTasks_NonexistentList_ReturnsNotFound() {
        var listId = Guid.NewGuid();
        _taskListServiceMock.Setup(x => x.GetListTasksAsync(listId))
            .ReturnsAsync((IEnumerable<TaskReadDto>?)null);

        var result = await _controller.GetListTasks(listId);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetListTasks_UnauthorizedAccess_ReturnsForbid() {
        var listId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var tasks = new List<TaskReadDto> { new() { ProjectId = projectId } };

        _taskListServiceMock.Setup(x => x.GetListTasksAsync(listId))
            .ReturnsAsync(tasks);
        _authServiceMock.Setup(x => x.AuthorizeProjectMembershipAsync(_userId, projectId))
            .ReturnsAsync(false);

        var result = await _controller.GetListTasks(listId);

        Assert.IsType<ForbidResult>(result);
    }
}
