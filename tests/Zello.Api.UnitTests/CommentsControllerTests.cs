using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Api.Controllers;
using Zello.Application.Features.Comments.Models;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Entities.Requests;
using Zello.Infrastructure.Data;

namespace Zello.Api.UnitTests;

public class CommentsControllerTests : IDisposable {
    private readonly CommentsController _controller;
    private readonly ApplicationDbContext _context;
    private Guid _testTaskId;
    private Guid _testUserId;
    private Guid _testProjectId;
    private Guid _testListId;

    public CommentsControllerTests() {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _controller = new CommentsController(_context);
        SeedTestData().GetAwaiter().GetResult();
        SetupControllerContext();
    }

    private void SetupControllerContext() {
        var claims = new[] {
            new Claim("UserId", _testUserId.ToString()),
            new Claim("AccessLevel", AccessLevel.Member.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext {
                User = principal
            }
        };
    }

    private async Task SeedTestData() {
        // Create and save user
        var user = new User {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@test.com",
            Name = "Test User",
            PasswordHash = "hashedpassword123",
            AccessLevel = AccessLevel.Member
        };
        _testUserId = user.Id;

        // Create workspace and member
        var workspace = new Workspace {
            Id = Guid.NewGuid(),
            Name = "Test Workspace"
        };

        var workspaceMember = new WorkspaceMember {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            UserId = user.Id
        };

        // Create project with list and task
        _testProjectId = Guid.NewGuid();
        _testListId = Guid.NewGuid();

        var project = new Project {
            Id = _testProjectId,
            Name = "Test Project",
            WorkspaceId = workspace.Id
        };

        var projectMember = new ProjectMember {
            Id = Guid.NewGuid(),
            ProjectId = _testProjectId,
            WorkspaceMemberId = workspaceMember.Id
        };

        var list = new TaskList {
            Id = _testListId,
            Name = "Test List",
            ProjectId = _testProjectId
        };

        var task = new WorkTask {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            ProjectId = _testProjectId,
            ListId = _testListId
        };
        _testTaskId = task.Id;

        // Save all entities
        await _context.Users.AddAsync(user);
        await _context.Workspaces.AddAsync(workspace);
        await _context.WorkspaceMembers.AddAsync(workspaceMember);
        await _context.Projects.AddAsync(project);
        await _context.ProjectMembers.AddAsync(projectMember);
        await _context.Lists.AddAsync(list);
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetComments_ReturnsOkResult() {
        var result = await _controller.GetComments();
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<Comment>>(okResult.Value);
    }

    [Fact]
    public async Task GetCommentById_WithInvalidId_ReturnsNotFound() {
        var result = await _controller.GetCommentById(Guid.NewGuid());
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateComment_WithValidData_ReturnsCreatedResult() {
        var request = new CreateCommentRequest {
            TaskId = _testTaskId,
            Content = "Test comment"
        };

        var result = await _controller.CreateComment(request);
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var comment = Assert.IsType<Comment>(createdResult.Value);
        Assert.Equal(request.Content, comment.Content);
    }

    [Fact]
    public async Task UpdateComment_WithValidData_ReturnsOkResult() {
        // Create a comment first
        var comment = new Comment {
            Id = Guid.NewGuid(),
            TaskId = _testTaskId,
            UserId = _testUserId,
            Content = "Original content"
        };
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();

        var request = new UpdateCommentRequest { Content = "Updated content" };
        var result = await _controller.UpdateComment(comment.Id, request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var updatedComment = Assert.IsType<Comment>(okResult.Value);
        Assert.Equal(request.Content, updatedComment.Content);
    }

    [Fact]
    public async Task DeleteComment_WithExistingComment_ReturnsNoContent() {
        // Create a comment first
        var comment = new Comment {
            Id = Guid.NewGuid(),
            TaskId = _testTaskId,
            UserId = _testUserId,
            Content = "Test content"
        };
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteComment(comment.Id);
        Assert.IsType<NoContentResult>(result);

        // Verify comment was actually deleted
        var deletedComment = await _context.Comments.FindAsync(comment.Id);
        Assert.Null(deletedComment);
    }
}
