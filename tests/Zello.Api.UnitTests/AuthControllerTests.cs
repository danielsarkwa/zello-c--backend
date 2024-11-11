using Microsoft.AspNetCore.Mvc;
using Xunit;
using Zello.Api.Controllers;

namespace Zello.Api.UnitTests;

public class AuthControllerTests {
    private readonly AuthController _controller;

    public AuthControllerTests() {
        _controller = new AuthController();
    }

    [Fact]
    public void Register_Returns200OK() {
        // Arrange
        var user = new { Email = "test@example.com" };

        // Act
        var result = _controller.Register(user);

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("User registered", contentResult.Content);
    }

    [Fact]
    public void Login_Returns200OK() {
        // Arrange
        var credentials = new { Email = "test@example.com", Password = "password" };

        // Act
        var result = _controller.Login(credentials);

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("User logged in", contentResult.Content);
    }

    [Fact]
    public void RefreshToken_Returns200OK() {
        // Arrange
        var token = new { RefreshToken = "token" };

        // Act
        var result = _controller.RefreshToken(token);

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("Token refreshed", contentResult.Content);
    }

    [Fact]
    public void Logout_Returns200OK() {
        // Act
        var result = _controller.Logout();

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("User logged out", contentResult.Content);
    }

    [Fact]
    public void ChangePassword_Returns200OK() {
        // Arrange
        var request = new { NewPassword = "newpassword" };

        // Act
        var result = _controller.ChangePassword(request);

        // Assert
        Assert.IsType<ContentResult>(result);
        var contentResult = (ContentResult)result;
        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("application/json", contentResult.ContentType);
        Assert.Contains("Password changed", contentResult.Content);
    }
}
