using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Api.Controllers;
using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Data;

namespace Zello.Api.UnitTests;

public class WorkspacesControllerTests : IDisposable {
    private readonly WorkspacesController _controller;
    private readonly ApplicationDbContext _context;
    private readonly Guid _userId;

    public WorkspacesControllerTests() {
        // Setup mock context
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _controller = new WorkspacesController(_context);

        // Setup the controller context with user claims
        _userId = Guid.NewGuid();
        var claims = new List<Claim> {
            new Claim("UserId", _userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Add test user to context with all required fields
        _context.Users.Add(new User {
            Id = _userId,
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword123",
            CreatedDate = DateTime.UtcNow
        });
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateWorkspace_ValidInput_ReturnsCreatedResult() {
        // Arrange
        var createDto = new WorkspaceCreateDto {
            Name = "Test Workspace"
        };

        // Act
        var actionResult = await _controller.CreateWorkspace(createDto);

        // Assert
        var result = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var value = result.Value;

        // Use reflection to get properties since it's an anonymous type
        var type = value.GetType();
        var nameProperty = type.GetProperty("Name");
        var ownerIdProperty = type.GetProperty("OwnerId");
        var idProperty = type.GetProperty("Id");

        Assert.NotNull(nameProperty);
        Assert.NotNull(ownerIdProperty);
        Assert.NotNull(idProperty);

        Assert.Equal(createDto.Name, nameProperty.GetValue(value));
        Assert.Equal(_userId, ownerIdProperty.GetValue(value));
        Assert.IsType<Guid>(idProperty.GetValue(value));
    }

    [Fact]
    public async Task CreateWorkspace_NullInput_ReturnsBadRequest() {
        // Act
        var result = await _controller.CreateWorkspace(null);

        // Assert
        Assert.IsType<BadRequestResult>(result.Result);
    }

    [Fact]
    public async Task GetAllWorkspaces_ReturnsOkResult() {
        // Act
        var result = await _controller.GetAllWorkspaces();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<List<WorkspaceReadDto>>(okResult.Value);
    }

    [Fact]
    public async Task GetWorkspace_ReturnsNotFoundForNonexistentId() {
        // Act
        var result = await _controller.GetWorkspace(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetWorkspace_ExistingWorkspace_ReturnsOkWithWorkspace() {
        // Arrange
        var workspace = new Workspace {
            Id = Guid.NewGuid(),
            Name = "Test Workspace",
            OwnerId = _userId,
            CreatedDate = DateTime.UtcNow
        };
        await _context.Workspaces.AddAsync(workspace);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetWorkspace(workspace.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<Workspace>(okResult.Value);
        Assert.Equal(workspace.Id, returnValue.Id);
        Assert.Equal(workspace.Name, returnValue.Name);
    }

    [Fact]
    public async Task UpdateWorkspace_ReturnsNotFoundForNonexistentId() {
        // Arrange
        var updateDto = new WorkspaceUpdateDto { Name = "Updated Name" };

        // Act
        var result = await _controller.UpdateWorkspace(Guid.NewGuid(), updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteWorkspace_ReturnsNotFoundForNonexistentId() {
        // Act
        var result = await _controller.DeleteWorkspace(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddWorkspaceMember_ReturnsNotFoundForNonexistentWorkspace() {
        // Arrange
        var createMemberDto = new WorkspaceMemberCreateDto {
            UserId = Guid.NewGuid(),
            AccessLevel = AccessLevel.Member
        };

        // Act
        var result = await _controller.AddWorkspaceMember(Guid.NewGuid(), createMemberDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
