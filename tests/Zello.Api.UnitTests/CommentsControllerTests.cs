using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Zello.Api.Controllers;
using Zello.Application.Dtos;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities.Api.User;

public class CommentsControllerTests {
    private readonly CommentsController _controller;
    private readonly Mock<ICommentService> _mockCommentService;
    private readonly Mock<IAuthorizationService> _mockAuthorizationService;
    private readonly Guid _testTaskId = Guid.NewGuid();
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProjectId = Guid.NewGuid();

    public CommentsControllerTests() {
        _mockCommentService = new Mock<ICommentService>();
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _controller =
            new CommentsController(_mockCommentService.Object, _mockAuthorizationService.Object);
        SetupControllerContext();
    }

    private void SetupControllerContext() {
        var claims = new List<Claim> {
            new Claim("UserId", _testUserId.ToString()),
            new Claim("AccessLevel", AccessLevel.Member.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext();
        httpContext.User = claimsPrincipal;

        _controller.ControllerContext = new ControllerContext() {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task CreateComment_WithValidData_ReturnsCreatedResult() {
        // Arrange
        var createDto = new CommentCreateDto {
            TaskId = _testTaskId,
            Content = "Test comment"
        };
        var createdComment = new CommentReadDto {
            Id = Guid.NewGuid(),
            TaskId = _testTaskId,
            Content = "Test comment",
            UserId = _testUserId,
            CreatedDate = DateTime.UtcNow
        };

        _mockCommentService
            .Setup(x => x.CreateCommentAsync(It.IsAny<CommentCreateDto>(), _testUserId))
            .ReturnsAsync(createdComment);

        // Add this mock setup
        _mockCommentService
            .Setup(x => x.GetTaskProjectDetailsAsync(_testTaskId))
            .ReturnsAsync(new TaskProjectDetailsDto {
                TaskId = _testTaskId,
                ProjectId = _testProjectId
            });

        _mockAuthorizationService
            .Setup(x =>
                x.AuthorizeProjectAccessAsync(_testUserId, _testProjectId, AccessLevel.Member))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateComment(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedComment = Assert.IsType<CommentReadDto>(createdResult.Value);
        Assert.Equal(createdComment.Content, returnedComment.Content);
    }

    [Fact]
    public async Task UpdateComment_WithValidData_ReturnsOkResult() {
        // Arrange
        var commentId = Guid.NewGuid();
        var updateDto = new CommentUpdateDto { Content = "Updated content" };
        var updatedComment = new CommentReadDto {
            Id = commentId,
            TaskId = _testTaskId,
            Content = "Updated content",
            UserId = _testUserId,
            CreatedDate = DateTime.UtcNow
        };

        _mockCommentService
            .Setup(x => x.GetCommentByIdAsync(commentId))
            .ReturnsAsync(updatedComment);

        // Add this mock setup
        _mockCommentService
            .Setup(x => x.GetTaskProjectDetailsAsync(_testTaskId))
            .ReturnsAsync(new TaskProjectDetailsDto {
                TaskId = _testTaskId,
                ProjectId = _testProjectId
            });

        _mockCommentService
            .Setup(x => x.UpdateCommentAsync(commentId, updateDto))
            .ReturnsAsync(updatedComment);

        _mockAuthorizationService
            .Setup(x =>
                x.AuthorizeProjectAccessAsync(_testUserId, _testProjectId, AccessLevel.Member))
            .ReturnsAsync(true);


        // Act
        var result = await _controller.UpdateComment(commentId, updateDto);

        // Debug info
        if (result is BadRequestObjectResult badResult) {
            var message = badResult.Value?.ToString();
            Assert.False(true, $"Got BadRequest with message: {message}");
        }

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedComment = Assert.IsType<CommentReadDto>(okResult.Value);
        Assert.Equal(updateDto.Content, returnedComment.Content);
    }

    [Fact]
    public async Task DeleteComment_WithValidId_ReturnsNoContent() {
        // Arrange
        var commentId = Guid.NewGuid();
        var comment = new CommentReadDto {
            Id = commentId,
            TaskId = _testTaskId,
            UserId = _testUserId,
            Content = "Test comment",
            CreatedDate = DateTime.UtcNow
        };

        // Mock getting the comment
        _mockCommentService
            .Setup(x => x.GetCommentByIdAsync(commentId))
            .ReturnsAsync(comment);

        // Mock getting task project details
        _mockCommentService
            .Setup(x => x.GetTaskProjectDetailsAsync(_testTaskId))
            .ReturnsAsync(new TaskProjectDetailsDto {
                TaskId = _testTaskId,
                ProjectId = _testProjectId
            });

        // Mock project access authorization
        _mockAuthorizationService
            .Setup(x =>
                x.AuthorizeProjectAccessAsync(_testUserId, _testProjectId, AccessLevel.Member))
            .ReturnsAsync(true);

        // Mock delete operation
        _mockCommentService
            .Setup(x => x.DeleteCommentAsync(commentId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }


    [Fact]
    public async Task DeleteComment_WithUnauthorizedUser_ReturnsForbid() {
        // Arrange
        var commentId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid(); // Different from _testUserId
        var comment = new CommentReadDto {
            Id = commentId,
            TaskId = _testTaskId,
            UserId = differentUserId, // Different user owns the comment
            Content = "Test comment",
            CreatedDate = DateTime.UtcNow
        };

        // Mock getting the comment
        _mockCommentService
            .Setup(x => x.GetCommentByIdAsync(commentId))
            .ReturnsAsync(comment);

        // Mock getting task project details
        _mockCommentService
            .Setup(x => x.GetTaskProjectDetailsAsync(_testTaskId))
            .ReturnsAsync(new TaskProjectDetailsDto {
                TaskId = _testTaskId,
                ProjectId = _testProjectId
            });

        // Mock project access authorization (user has project access but doesn't own comment)
        _mockAuthorizationService
            .Setup(x =>
                x.AuthorizeProjectAccessAsync(_testUserId, _testProjectId, AccessLevel.Member))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }
}
