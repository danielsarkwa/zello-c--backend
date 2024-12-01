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

public class ProjectControllerTests {
    private readonly ProjectController _controller;
    private readonly Mock<IProjectService> _projectServiceMock;
    private readonly Mock<IAuthorizationService> _authServiceMock;
    private readonly Guid _userId;
    private readonly AccessLevel _userAccess;

    public ProjectControllerTests() {
        _projectServiceMock = new Mock<IProjectService>();
        _authServiceMock = new Mock<IAuthorizationService>();
        _controller = new ProjectController(_projectServiceMock.Object, _authServiceMock.Object);
        _userId = Guid.NewGuid();
        _userAccess = AccessLevel.Admin;

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
    public async Task CreateProject_ValidInput_ReturnsCreatedResult() {
        // Arrange
        var createDto = new ProjectCreateDto {
            Name = "Test Project",
            WorkspaceId = Guid.NewGuid()
        };
        var expectedProject = new ProjectReadDto {
            Id = Guid.NewGuid(),
            Name = createDto.Name
        };

        _authServiceMock.Setup(x =>
                x.AuthorizeWorkspaceMembershipAsync(createDto.WorkspaceId, _userId))
            .ReturnsAsync(true);
        _authServiceMock.Setup(x =>
                x.AuthorizeProjectAccessAsync(_userId, createDto.WorkspaceId, AccessLevel.Member))
            .ReturnsAsync(true);
        _projectServiceMock.Setup(x => x.CreateProjectAsync(createDto, _userId))
            .ReturnsAsync(expectedProject);

        // Act
        var result = await _controller.CreateProject(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(expectedProject, createdResult.Value);
    }

    [Fact]
    public async Task GetProjectById_ExistingProject_ReturnsOk() {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new ProjectReadDto { Id = projectId, Name = "Test" };

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);
        _authServiceMock.Setup(x => x.AuthorizeProjectMembershipAsync(_userId, projectId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetProjectById(projectId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(project, okResult.Value);
    }

    [Fact]
    public async Task GetProjectById_UnauthorizedAccess_ReturnsForbid() {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new ProjectReadDto { Id = projectId };

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);
        _authServiceMock.Setup(x => x.AuthorizeProjectMembershipAsync(_userId, projectId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetProjectById(projectId);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateProjectMemberAccess_ValidInput_ReturnsOk() {
        // Arrange
        var elevation = new MemberElevationDto {
            MemberId = Guid.NewGuid(),
            NewAccessLevel = AccessLevel.Member
        };
        var updatedMember = new ProjectMember { Id = elevation.MemberId };

        _projectServiceMock.Setup(x => x.UpdateMemberAccessAsync(
                elevation.MemberId, elevation.NewAccessLevel, _userId, _userAccess))
            .ReturnsAsync(updatedMember);

        // Act
        var result = await _controller.UpdateProjectMemberAccess(elevation);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task CreateList_ValidInput_ReturnsCreatedResult() {
        // Arrange
        var projectId = Guid.NewGuid();
        var createDto = new ListCreateDto {
            Name = "Test List",
            Tasks = new List<TaskCreateDto> {
                new() {
                    Name = "Test Task",
                    Description = "Test Description",
                    Status = CurrentTaskStatus.NotStarted,
                    Priority = Priority.Medium
                }
            }
        };

        var createdList = new TaskList {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Name = createDto.Name,
            Position = 1,
            CreatedDate = DateTime.UtcNow
        };

        _projectServiceMock.Setup(x => x.CreateListAsync(projectId, createDto))
            .ReturnsAsync(createdList);
        _authServiceMock.Setup(x => x.AuthorizeProjectMembershipAsync(_userId, projectId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateList(projectId, createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var resultDto = Assert.IsType<ListReadDto>(createdResult.Value);
        Assert.Equal(createDto.Name, resultDto.Name);
        Assert.Equal(projectId, resultDto.ProjectId);
    }

    [Fact]
    public async Task CreateList_UnauthorizedAccess_ReturnsForbid() {
        // Arrange
        var projectId = Guid.NewGuid();
        var createDto = new ListCreateDto { Name = "Test List" };

        _authServiceMock.Setup(x => x.AuthorizeProjectMembershipAsync(_userId, projectId))
            .ReturnsAsync(false);

        // We don't need to setup project service because auth check will fail first

        // Act
        var result = await _controller.CreateList(projectId, createDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
        // Verify that CreateListAsync was never called
        _projectServiceMock.Verify(
            x => x.CreateListAsync(It.IsAny<Guid>(), It.IsAny<ListCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateList_ProjectNotFound_ReturnsNotFound() {
        // Arrange
        var projectId = Guid.NewGuid();
        var createDto = new ListCreateDto { Name = "Test List" };

        _authServiceMock.Setup(x => x.AuthorizeProjectMembershipAsync(_userId, projectId))
            .ReturnsAsync(true);

        _projectServiceMock.Setup(x => x.CreateListAsync(projectId, createDto))
            .ThrowsAsync(new KeyNotFoundException($"Project with ID {projectId} not found"));

        // Act
        var result = await _controller.CreateList(projectId, createDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetProjectLists_ExistingProject_ReturnsOk() {
        // Arrange
        var projectId = Guid.NewGuid();
        var lists = new List<TaskList> {
            new() { Id = Guid.NewGuid(), Name = "List 1" },
            new() { Id = Guid.NewGuid(), Name = "List 2" }
        };

        _projectServiceMock.Setup(x => x.GetProjectListsAsync(projectId))
            .ReturnsAsync(lists);
        _authServiceMock.Setup(x => x.AuthorizeProjectMembershipAsync(_userId, projectId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetProjectLists(projectId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task AddProjectMember_ValidInput_ReturnsCreatedResult() {
        // Arrange
        var createDto = new ProjectMemberCreateDto {
            ProjectId = Guid.NewGuid(),
            WorkspaceMemberId = Guid.NewGuid(),
            AccessLevel = AccessLevel.Member
        };
        var createdMember = new ProjectMember { Id = Guid.NewGuid() };

        _projectServiceMock.Setup(x => x.AddProjectMemberAsync(createDto, _userId))
            .ReturnsAsync(createdMember);

        // Act
        var result = await _controller.AddProjectMember(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.NotNull(createdResult.Value);
    }

    [Fact]
    public async Task DeleteProject_ExistingProject_ReturnsNoContent() {
        // Arrange
        var projectId = Guid.NewGuid();
        var project = new ProjectReadDto {
            Id = projectId,
            Members = new List<ProjectMemberReadDto> {
                new() { WorkspaceMember = new() { UserId = _userId } }
            }
        };

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(projectId))
            .ReturnsAsync(project);
        _authServiceMock.Setup(x => x.AuthorizeProjectMembershipAsync(_userId, projectId))
            .ReturnsAsync(true);
        _projectServiceMock.Setup(x => x.DeleteProjectAsync(projectId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteProject(projectId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}
