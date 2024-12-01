using System;
using Xunit;
using Zello.Domain.Entities;

namespace Zello.Domain.UnitTests;

public class CommentEntityTests {
    [Fact]
    public void CreateComment_WithValidData_ReturnsValidCommentObject() {
        // Arrange & Act
        var comment = new Comment {
            Id = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Content = "This is a test comment.",
            CreatedDate = DateTime.UtcNow
        };

        // Assert
        Assert.NotEqual(Guid.Empty, comment.Id);
        Assert.NotEqual(Guid.Empty, comment.TaskId);
        Assert.NotEqual(Guid.Empty, comment.UserId);
        Assert.Equal("This is a test comment.", comment.Content);
        Assert.True(DateTime.UtcNow.Subtract(comment.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void AssignTaskAndUser_ToComment_AssignsSuccessfully() {
        // Arrange
        var task = new WorkTask {
            Id = Guid.NewGuid(),
            Name = "Test Task",
            ProjectId = Guid.NewGuid(),
            ListId = Guid.NewGuid()
        };

        var user = new User {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };

        var comment = new Comment {
            TaskId = task.Id,
            UserId = user.Id,
            Task = task,
            User = user,
            Content = "This is a test comment."
        };

        // Assert
        Assert.Equal(task.Id, comment.TaskId);
        Assert.Equal(user.Id, comment.UserId);
        Assert.Equal(task, comment.Task);
        Assert.Equal(user, comment.User);
    }

    [Fact]
    public void CreateComment_DefaultCreatedDate_IsSetToUtcNow() {
        // Arrange & Act
        var comment = new Comment {
            TaskId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Content = "Test Comment"
        };

        // Assert
        Assert.True(DateTime.UtcNow.Subtract(comment.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void CreateComment_WithoutContent_ReturnsEmptyContent() {
        // Arrange & Act
        var comment = new Comment {
            TaskId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        // Assert
        Assert.Equal(string.Empty, comment.Content);
    }
}
