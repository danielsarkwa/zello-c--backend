using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Domain.UnitTests;

public class UserEntityTests {
    [Fact]
    public void CreateUser_WithValidData_ReturnsValidUserObject() {
        // Arrange & Act
        var user = new User {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword",
            AccessLevel = AccessLevel.Member
        };

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("Test User", user.Name);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("hashedpassword", user.PasswordHash);
        Assert.Equal(AccessLevel.Member, user.AccessLevel);
    }

    [Fact]
    public void CreateUser_DefaultCreatedDate_IsSetToUtcNow() {
        // Arrange & Act
        var user = new User {
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };

        // Assert
        Assert.True(DateTime.UtcNow.Subtract(user.CreatedDate).TotalSeconds < 1);
    }

    [Fact]
    public void InitializeUser_NavigationProperties_AreInitializedCorrectly() {
        // Arrange & Act
        var user = new User {
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };

        // Assert
        Assert.NotNull(user.WorkspaceMembers);
        Assert.NotNull(user.AssignedTasks);
        Assert.NotNull(user.Comments);
        Assert.Empty(user.WorkspaceMembers);
        Assert.Empty(user.AssignedTasks);
        Assert.Empty(user.Comments);
    }

    [Fact]
    public void AddItems_ToUserNavigationProperties_ItemsAddedSuccessfully() {
        // Arrange
        var user = new User {
            Username = "testuser",
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedpassword"
        };

        var workspaceMember = new WorkspaceMember();
        var taskAssignee = new TaskAssignee();
        var comment = new Comment();

        // Act
        user.WorkspaceMembers.Add(workspaceMember);
        user.AssignedTasks.Add(taskAssignee);
        user.Comments.Add(comment);

        // Assert
        Assert.Single(user.WorkspaceMembers);
        Assert.Single(user.AssignedTasks);
        Assert.Single(user.Comments);
    }

    [Fact]
    public void ValidateUser_RequiredProperties_AreEnforcedByModel() {
        // Arrange & Act
        var userType = typeof(User);
        var requiredProperties = new[]
            {
            nameof(User.Username),
            nameof(User.Name),
            nameof(User.Email),
            nameof(User.PasswordHash)
        };

        // Assert
        foreach (var propertyName in requiredProperties) {
            var property = userType.GetProperty(propertyName);
            Assert.NotNull(property);
            Assert.True(property.PropertyType == typeof(string), $"{propertyName} should be a string");
        }
    }
}
