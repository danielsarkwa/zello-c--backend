using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Api.Controllers;
using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Enums;
using Zello.Infrastructure.Data;

namespace Zello.Api.UnitTests;

public class ProjectControllerTests : IDisposable {
    private readonly ProjectController _controller;
    private readonly ApplicationDbContext _context;
    private readonly Guid _userId;
    private readonly Guid _workspaceId;
    private readonly Guid _workspaceMemberId;

    public ProjectControllerTests() {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _controller = new ProjectController(_context);

        // Setup user and claims
        _userId = Guid.NewGuid();
        var claims = new List<Claim> {
            new Claim("UserId", _userId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Setup test data
        _workspaceId = Guid.NewGuid();
        _workspaceMemberId = Guid.NewGuid();
        SetupTestData();
    }

    private void SetupTestData() {
        // Add test user
        _context.Users.Add(new User {
            Id = _userId,
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword123",
            CreatedDate = DateTime.UtcNow
        });

        // Add test workspace
        _context.Workspaces.Add(new Workspace {
            Id = _workspaceId,
            Name = "Test Workspace",
            OwnerId = _userId,
            CreatedDate = DateTime.UtcNow
        });

        // Add workspace member
        _context.WorkspaceMembers.Add(new WorkspaceMember {
            Id = _workspaceMemberId,
            WorkspaceId = _workspaceId,
            UserId = _userId,
            AccessLevel = AccessLevel.Owner,
            CreatedDate = DateTime.UtcNow
        });

        _context.SaveChanges();
    }

    [Fact]
    public async Task CreateProject_ValidInput_ReturnsCreatedResult() {
        // Arrange
        var createDto = new ProjectCreateDto {
            Name = "Test Project",
            Description = "Test Description",
            WorkspaceId = _workspaceId,
            Status = ProjectStatus.NotStarted
        };

        // Act
        var result = await _controller.CreateProject(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var projectDto = Assert.IsType<ProjectReadDto>(createdResult.Value);
        Assert.Equal(createDto.Name, projectDto.Name);
        Assert.Equal(createDto.WorkspaceId, projectDto.WorkspaceId);
    }

    [Fact]
    public async Task CreateProject_InvalidWorkspace_ReturnsBadRequest() {
        // Arrange
        var createDto = new ProjectCreateDto {
            Name = "Test Project",
            WorkspaceId = Guid.NewGuid(),
            Status = ProjectStatus.NotStarted
        };

        // Act
        var result = await _controller.CreateProject(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid workspace ID", badRequestResult.Value);
    }

    [Fact]
    public async Task GetProjectById_ExistingProject_ReturnsProject() {
        // Arrange
        var project = new Project {
            Id = Guid.NewGuid(),
            WorkspaceId = _workspaceId,
            Name = "Test Project",
            Status = ProjectStatus.NotStarted,
            CreatedDate = DateTime.UtcNow
        };
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProjectById(project.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProject = Assert.IsType<Project>(okResult.Value);
        Assert.Equal(project.Id, returnedProject.Id);
    }

    [Fact]
    public async Task CreateList_ValidInput_ReturnsCreatedResult()
    {
        // Arrange
        var project = new Project {
            Id = Guid.NewGuid(),
            WorkspaceId = _workspaceId,
            Name = "Test Project",
            Status = ProjectStatus.NotStarted,
            CreatedDate = DateTime.UtcNow
        };
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        var createDto = new ListCreateDto {
            ProjectId = project.Id,
            Name = "Test List",
            Position = 0
        };

        // Act
        var result = await _controller.CreateList(project.Id, createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var list = Assert.IsType<TaskList>(createdResult.Value);
        Assert.Equal(createDto.Name, list.Name);
        Assert.Equal(0, list.Position);
    }

    [Fact]
    public async Task CreateList_NonexistentProject_ReturnsNotFound()
    {
        // Arrange
        var createDto = new ListCreateDto {
            ProjectId = Guid.NewGuid(),
            Name = "Test List",
            Position = 0
        };

        // Act
        var result = await _controller.CreateList(Guid.NewGuid(), createDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetProjectLists_ExistingProject_ReturnsLists()
    {
        // Arrange
        var project = new Project {
            Id = Guid.NewGuid(),
            WorkspaceId = _workspaceId,
            Name = "Test Project",
            Status = ProjectStatus.NotStarted,
            CreatedDate = DateTime.UtcNow
        };
        await _context.Projects.AddAsync(project);

        var list = new TaskList {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Name = "Test List",
            Position = 0,
            CreatedDate = DateTime.UtcNow
        };
        await _context.Lists.AddAsync(list);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetProjectLists(project.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var lists = Assert.IsType<List<TaskList>>(okResult.Value);
        Assert.Single(lists);
        Assert.Equal(list.Name, lists[0].Name);
    }

    [Fact]
    public async Task GetProjectLists_NonexistentProject_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetProjectLists(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
