using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Zello.Api.Controllers;
using Zello.Domain.Entities.Api.User;
using Zello.Application.Features.Authentication.Models;
using Zello.Domain.Entities.Api.Message;

namespace Zello.Api.UnitTests;

public class AuthControllerTests {
    private readonly AuthController _controller;
    private readonly IConfiguration _configuration;

    public AuthControllerTests() {
        // Fix CS8620 by explicitly creating a Dictionary<string, string?> instead of Dictionary<string, string>
        var inMemorySettings = new Dictionary<string, string?> {
            { "Jwt:Key", "your-super-secret-key-with-at-least-32-characters" },
            { "Jwt:Issuer", "https://localhost:4321" },
            { "Jwt:Audience", "https://localhost:4321" },
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _controller = new AuthController(_configuration);
    }

    [Fact]
    public void Register_ValidUser_Returns200OK() {
        // Arrange
        var user = new RegisterUserRequest {
            Username = "test",
            Email = "test@test.com",
            Password = "Test123!@#",
            Name = "John"
        };

        // Act
        var result = _controller.Register(user);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var response = Assert.IsType<SimpleMessage>(okResult.Value);
        Assert.Equal("User registered successfully", response.Message);
    }

    [Fact]
    public void Login_ValidCredentials_ReturnsOkWithToken() {
        // Arrange
        var request = new LoginRequest {
            Username = "test",
            Password = "Test123!@#"
        };

        // Act
        var result = _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.NotEmpty(response.Token); // Fix xUnit2002: Instead of Assert.NotNull on DateTime
        Assert.Equal("Bearer", response.TokenType);
        // Remove Assert.NotNull on DateTime since it's a value type
        Assert.True(response.Expires > DateTime.MinValue); // Alternative check for DateTime
    }

    [Fact]
    public void Login_EmptyCredentials_ReturnsBadRequest() {
        // Arrange
        var request = new LoginRequest {
            Username = "",
            Password = ""
        };

        // Act
        var result = _controller.Login(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);

        var response = Assert.IsAssignableFrom<object>(badRequestResult.Value);
        var message = response.GetType().GetProperty("Message")?.GetValue(response) as string;
        Assert.Equal("Username and password are required", message);
    }

    [Fact]
    public void Logout_ReturnsOkResult() {
        // Act
        var result = _controller.Logout();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var response = Assert.IsType<SimpleMessage>(okResult.Value);
        Assert.Equal("Logged out successfully", response.Message);
    }

    [Theory]
    [InlineData("")] // Fix xUnit1012: Remove the null test case since username is non-nullable
    public void Login_InvalidCredentials_ReturnsBadRequest(string username) {
        // Arrange
        var request = new LoginRequest {
            Username = username,
            Password = "password"
        };

        // Act
        var result = _controller.Login(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}
