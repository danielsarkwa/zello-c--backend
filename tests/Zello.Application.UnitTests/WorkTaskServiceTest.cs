using Moq;
using Zello.Application.Dtos;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.RepositoryInterfaces;

namespace Zello.Application.UnitTests;

public class WorkTaskServiceTests {
    private readonly Mock<IWorkTaskRepository> _mockWorkTaskRepository;
    private readonly Mock<ITaskAssigneeRepository> _mockTaskAssigneeRepository;
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly Mock<IWorkspaceMemberRepository> _mockWorkspaceMemberRepository;
    private readonly Mock<IUserService> _mockUserService;
    private readonly WorkTaskService _workTaskService;

    public WorkTaskServiceTests() {
        _mockWorkTaskRepository = new Mock<IWorkTaskRepository>();
        _mockTaskAssigneeRepository = new Mock<ITaskAssigneeRepository>();
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockWorkspaceMemberRepository = new Mock<IWorkspaceMemberRepository>();
        _mockUserService = new Mock<IUserService>();

        _workTaskService = new WorkTaskService(
            _mockWorkTaskRepository.Object,
            new Mock<ITaskListRepository>().Object,
            _mockWorkspaceMemberRepository.Object,
            _mockTaskAssigneeRepository.Object,
            _mockProjectRepository.Object,
            new Mock<ICommentService>().Object,
            _mockUserService.Object
        );
    }

    [Fact]
    public async Task AssignUserToTaskAsync_ValidInputs_AddsAssignee() {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();

        var task = new WorkTask {
            Id = taskId,
            Project = new Project {
                WorkspaceId = workspaceId
            },
            Assignees = new List<TaskAssignee>()
        };

        var user = new User {
            Id = userId,
            Name = "Test User",
            Email = "testuser@example.com",
            Username = "testuser",
            CreatedDate = DateTime.UtcNow,
            PasswordHash = "hashedpassword123"
        };

        var userToAssign = UserReadDto.FromEntity(user);

        var workspaceMembers = new List<WorkspaceMember> {
            new() { UserId = requestingUserId, WorkspaceId = workspaceId },
            new() { UserId = userId, WorkspaceId = workspaceId }
        };

        var taskAssignee = new TaskAssignee {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = userId,
            AssignedDate = DateTime.UtcNow,
            User = user
        };

        _mockWorkTaskRepository.Setup(repo => repo.GetTaskByIdAsync(taskId))
            .ReturnsAsync(task);

        _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
            .ReturnsAsync(userToAssign);

        _mockWorkspaceMemberRepository.Setup(repo =>
                repo.GetMembersByWorkspaceIdAsync(workspaceId))
            .ReturnsAsync(workspaceMembers);

        _mockTaskAssigneeRepository.Setup(repo =>
                repo.AddAssigneeAsync(It.IsAny<TaskAssignee>()))
            .ReturnsAsync(taskAssignee);

        // Act
        var result = await _workTaskService.AssignUserToTaskAsync(taskId, userId, requestingUserId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskId, result.TaskId);
        Assert.Equal(userId, result.UserId);

        _mockWorkTaskRepository.Verify(repo =>
            repo.GetTaskByIdAsync(taskId), Times.Once);
        _mockUserService.Verify(service =>
            service.GetUserByIdAsync(userId), Times.Once);
        _mockWorkspaceMemberRepository.Verify(repo =>
            repo.GetMembersByWorkspaceIdAsync(workspaceId), Times.Once);
        _mockTaskAssigneeRepository.Verify(repo =>
            repo.AddAssigneeAsync(It.IsAny<TaskAssignee>()), Times.Once);
    }
}
