using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Zello.Api.Controllers;
using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Enums;
using Zello.Infrastructure.Data;

namespace Zello.Api.UnitTests;

public class ListControllerTests : IDisposable {
    private readonly ListController _controller;
    private readonly ApplicationDbContext _context;
    private Guid _testListId;
    private Guid _testProjectId;
    private Guid _testWorkspaceId;

    public ListControllerTests() {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _controller = new ListController(_context);
        SeedTestData().GetAwaiter().GetResult();
    }

    private async Task SeedTestData() {
        var workspace = new Workspace {
            Id = Guid.NewGuid(),
            Name = "Test Workspace"
        };
        _testWorkspaceId = workspace.Id;

        var project = new Project {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            Workspace = workspace,
            WorkspaceId = workspace.Id
        };
        _testProjectId = project.Id;

        var list = new TaskList {
            Id = Guid.NewGuid(),
            Name = "Test List",
            ProjectId = project.Id,
            Project = project,
            Position = 0
        };
        _testListId = list.Id;

        await _context.Workspaces.AddAsync(workspace);
        await _context.Projects.AddAsync(project);
        await _context.Lists.AddAsync(list);
        await _context.SaveChangesAsync();
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllLists_ReturnsOkResult() {
        var result = await _controller.GetAllLists();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var lists = Assert.IsType<List<ListReadDto>>(okResult.Value);
        Assert.Single(lists);
        Assert.Equal("Test List", lists[0].Name);
    }

    [Fact]
    public async Task GetAllLists_WithProjectFilter_ReturnsFilteredLists() {
        var result = await _controller.GetAllLists(_testProjectId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var lists = Assert.IsType<List<ListReadDto>>(okResult.Value);
        Assert.Single(lists);
        Assert.All(lists, l => Assert.Equal(_testProjectId, l.ProjectId));
    }

    [Fact]
    public async Task GetListById_WithValidId_ReturnsCorrectList() {
        var result = await _controller.GetListById(_testListId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var listDto = Assert.IsType<ListReadDto>(okResult.Value);
        Assert.Equal(_testListId, listDto.Id);
        Assert.Equal("Test List", listDto.Name);
    }

    [Fact]
    public async Task GetListById_WithInvalidId_ReturnsNotFound() {
        var result = await _controller.GetListById(Guid.NewGuid());
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateList_WithValidData_ReturnsUpdatedList() {
        var updateDto = new ListUpdateDto {
            Name = "Updated List",
            Position = 1,
            Id = _testListId // Changed from ProjectId since we don't need it in update
        };

        var result = await _controller.UpdateList(_testListId, updateDto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var listDto = Assert.IsType<ListReadDto>(okResult.Value);
        Assert.Equal("Updated List", listDto.Name);
        Assert.Equal(1, listDto.Position);
    }

    [Fact]
    public async Task UpdateList_WithInvalidId_ReturnsNotFound() {
        var updateDto = new ListUpdateDto {
            Name = "Updated List",
            Position = 1
        };

        var result = await _controller.UpdateList(Guid.NewGuid(), updateDto);
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateListPosition_WithValidData_UpdatesPositions() {
        // Add another list to make the position update valid
        var newList = new TaskList {
            Id = Guid.NewGuid(),
            Name = "Test List 2",
            ProjectId = _testProjectId,
            Position = 1
        };
        await _context.Lists.AddAsync(newList);
        await _context.SaveChangesAsync();

        var updateDto = new ListUpdateDto {
            Position = 1
        };

        var result = await _controller.UpdateListPosition(_testListId, updateDto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var listDto = Assert.IsType<ListReadDto>(okResult.Value);
        Assert.Equal(1, listDto.Position);
    }

    [Fact]
    public async Task UpdateListPosition_WithInvalidPosition_ReturnsBadRequest() {
        var updateDto = new ListUpdateDto {
            Position = -1
        };

        var result = await _controller.UpdateListPosition(_testListId, updateDto);
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateTask_ValidInput_ReturnsCreatedResult() {
        var userId = Guid.NewGuid();
        var workspaceMember = new WorkspaceMember {
            Id = Guid.NewGuid(),
            UserId = userId,
            WorkspaceId = _testWorkspaceId
        };

        await _context.WorkspaceMembers.AddAsync(workspaceMember);
        await _context.SaveChangesAsync();

        var createDto = new TaskCreateDto {
            Name = "New Task",
            ProjectId = _testProjectId,
            ListId = _testListId,
            Status = CurrentTaskStatus.NotStarted,
            Priority = Priority.Medium,
            Deadline = DateTime.UtcNow.AddDays(1)
        };

        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim("UserId", userId.ToString())
                }))
            }
        };

        var result = await _controller.CreateTask(_testListId, createDto);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var taskDto = Assert.IsType<TaskReadDto>(createdResult.Value);
    }


    [Fact]
    public async Task GetListTasks_WithValidId_ReturnsTasks() {
        var result = await _controller.GetListTasks(_testListId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var tasks = Assert.IsType<List<TaskReadDto>>(okResult.Value);
        Assert.All(tasks, t => Assert.Equal(_testListId, t.ListId));
    }

    [Fact]
    public async Task GetListTasks_WithInvalidId_ReturnsNotFound() {
        var result = await _controller.GetListTasks(Guid.NewGuid());
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
