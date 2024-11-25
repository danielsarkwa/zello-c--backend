using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Zello.Api.Controllers;
using Zello.Domain.Entities.Dto;
using Zello.Application.Features.Workspaces;
using Zello.Domain.Entities.Api.User;

namespace Zello.Api.UnitTests;

public class WorkspacesControllerTests {
    private readonly WorkspacesController _controller;

    public WorkspacesControllerTests() {
        _controller = new WorkspacesController();

        // Setup the controller context with user claims
        var userId = Guid.NewGuid();
        var claims = new List<Claim> {
            new Claim("UserId", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public void CreateWorkspace_ValidInput_ReturnsCreatedResult() {
        // Arrange
        var createDto = new CreateWorkspaceDto(
            Name: "Test Workspace",
            OwnerId: Guid.NewGuid(),
            Description: "Test Description"
        );

        // Act
        var actionResult = _controller.CreateWorkspace(createDto);


        // Assert - first get the Result from ActionResult<T>
        var result = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var returnValue = Assert.IsType<WorkspaceDto>(result.Value);
        Assert.Equal(createDto.Name, returnValue.Name);
    }

    [Fact]
    public void CreateWorkspace_NullInput_ReturnsBadRequest() {
        // Act
        var result = _controller.CreateWorkspace(null);

        // Assert
        Assert.IsType<BadRequestResult>(result.Result);
    }

    [Fact]
    public void GetAllWorkspaces_ReturnsOkResult() {
        // Act
        var result = _controller.GetAllWorkspaces();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void GetWorkspace_ReturnsNotFoundForNonexistentId() {
        // Act
        var result = _controller.GetWorkspace(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void UpdateWorkspace_ReturnsNotFoundForNonexistentId() {
        // Arrange
        var updateDto = new UpdateWorkspaceDto { Name = "Updated Name" };

        // Act
        var result = _controller.UpdateWorkspace(Guid.NewGuid(), updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void DeleteWorkspace_ReturnsNotFoundForNonexistentId() {
        // Act
        var result = _controller.DeleteWorkspace(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void AddWorkspaceMember_ReturnsNotFoundForNonexistentWorkspace() {
        // Arrange
        var createMemberDto = new CreateWorkspaceMemberDto {
            UserId = Guid.NewGuid(),
            AccessLevel = AccessLevel.Member
        };

        // Act
        var result = _controller.AddWorkspaceMember(Guid.NewGuid(), createMemberDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
