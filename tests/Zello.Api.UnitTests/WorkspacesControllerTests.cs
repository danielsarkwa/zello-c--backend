using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Zello.Application.Dtos;
using Zello.Application.Exceptions;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities.Api.User;

namespace Zello.Api.UnitTests;

public class WorkspacesControllerTests {
    private readonly WorkspacesController _controller;
    private readonly Mock<IWorkspaceService> _workspaceServiceMock;
    private readonly Guid _userId;
    private readonly AccessLevel _userAccess;

    public WorkspacesControllerTests() {
        _workspaceServiceMock = new Mock<IWorkspaceService>();
        _controller = new WorkspacesController(_workspaceServiceMock.Object);
        _userId = Guid.NewGuid();
        _userAccess = AccessLevel.Admin;

        var claims = new List<Claim> {
            new Claim("UserId", _userId.ToString()),
            new Claim("AccessLevel", _userAccess.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task CreateWorkspace_ValidInput_ReturnsCreatedResult() {
        // Arrange
        var createDto = new WorkspaceCreateDto { Name = "Test Workspace" };
        var expectedWorkspace = new WorkspaceReadDto {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            OwnerId = _userId
        };

        _workspaceServiceMock.Setup(x => x.CreateWorkspaceAsync(createDto, _userId))
            .ReturnsAsync(expectedWorkspace);

        // Act
        var result = await _controller.CreateWorkspace(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(expectedWorkspace, createdResult.Value);
    }

    [Fact]
    public async Task GetAllWorkspaces_ReturnsOkWithWorkspaces() {
        // Arrange
        var workspaces = new List<WorkspaceReadDto> {
            new() { Id = Guid.NewGuid(), Name = "Workspace 1" },
            new() { Id = Guid.NewGuid(), Name = "Workspace 2" }
        };

        _workspaceServiceMock.Setup(x => x.GetAllWorkspacesAsync(_userId, _userAccess))
            .ReturnsAsync(workspaces);

        // Act
        var result = await _controller.GetAllWorkspaces();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(workspaces, okResult.Value);
    }

    [Fact]
    public async Task GetWorkspace_ExistingId_ReturnsOkWithWorkspace() {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var workspace = new WorkspaceReadDto { Id = workspaceId, Name = "Test" };

        _workspaceServiceMock.Setup(x => x.GetWorkspaceByIdAsync(workspaceId, _userId, _userAccess))
            .ReturnsAsync(workspace);

        // Act
        var result = await _controller.GetWorkspace(workspaceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(workspace, okResult.Value);
    }

    [Fact]
    public async Task GetWorkspace_NonexistentId_ReturnsNotFound() {
        // Arrange
        var workspaceId = Guid.NewGuid();
        _workspaceServiceMock.Setup(x => x.GetWorkspaceByIdAsync(workspaceId, _userId, _userAccess))
            .ThrowsAsync(new WorkspaceNotFoundException());

        // Act
        var result = await _controller.GetWorkspace(workspaceId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateWorkspace_ValidUpdate_ReturnsOkResult() {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var updateDto = new WorkspaceUpdateDto { Name = "Updated Name" };
        var updatedWorkspace = new WorkspaceReadDto { Id = workspaceId, Name = updateDto.Name };

        _workspaceServiceMock.Setup(x =>
                x.UpdateWorkspaceAsync(workspaceId, updateDto, _userId, _userAccess))
            .ReturnsAsync(updatedWorkspace);

        // Act
        var result = await _controller.UpdateWorkspace(workspaceId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updatedWorkspace, okResult.Value);
    }

    [Fact]
    public async Task DeleteWorkspace_ExistingWorkspace_ReturnsNoContent() {
        // Arrange
        var workspaceId = Guid.NewGuid();
        _workspaceServiceMock.Setup(x => x.DeleteWorkspaceAsync(workspaceId, _userId, _userAccess))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteWorkspace(workspaceId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AddWorkspaceMember_ValidMember_ReturnsCreatedResult() {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var createDto = new WorkspaceMemberCreateDto {
            UserId = Guid.NewGuid(),
            AccessLevel = AccessLevel.Member
        };
        var createdMember = new WorkspaceMemberReadDto {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            UserId = createDto.UserId
        };

        _workspaceServiceMock.Setup(x =>
                x.AddWorkspaceMemberAsync(workspaceId, createDto, _userId, _userAccess))
            .ReturnsAsync(createdMember);

        // Act
        var result = await _controller.AddWorkspaceMember(workspaceId, createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(createdMember, createdResult.Value);
    }

    [Fact]
    public async Task GetWorkspaceMembers_ExistingWorkspace_ReturnsOkWithMembers() {
        // Arrange
        var workspaceId = Guid.NewGuid();
        var members = new List<WorkspaceMemberReadDto> {
            new() { Id = Guid.NewGuid(), WorkspaceId = workspaceId },
            new() { Id = Guid.NewGuid(), WorkspaceId = workspaceId }
        };

        _workspaceServiceMock
            .Setup(x => x.GetWorkspaceMembersAsync(workspaceId, _userId, _userAccess))
            .ReturnsAsync(members);

        // Act
        var result = await _controller.GetWorkspaceMembers(workspaceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(members, okResult.Value);
    }

    [Fact]
    public async Task UpdateMemberAccess_ValidUpdate_ReturnsOkResult() {
        // Arrange
        var memberId = Guid.NewGuid();
        var updateDto = new WorkspaceMemberUpdateDto { Role = AccessLevel.Admin };
        var updatedMember = new WorkspaceMemberReadDto {
            Id = memberId,
            AccessLevel = updateDto.Role
        };

        _workspaceServiceMock.Setup(x =>
                x.UpdateMemberAccessAsync(memberId, updateDto, _userId, _userAccess))
            .ReturnsAsync(updatedMember);

        // Act
        var result = await _controller.UpdateMemberAccess(memberId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updatedMember, okResult.Value);
    }
}
