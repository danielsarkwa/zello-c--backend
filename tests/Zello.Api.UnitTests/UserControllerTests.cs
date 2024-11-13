using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Zello.Api.Controllers;
using Zello.Domain.Entities.Api.User;

namespace Zello.Api.UnitTests;

public class UserControllerTests {
    private readonly UserController _controller;

    public UserControllerTests() {
        _controller = new UserController();
    }

    [Fact]
    public void GetCurrentUser_WithAuthenticatedUser_ReturnsOkResultWithUserDto() {
        // Arrange
        const string username = "testuser@example.com";
        var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, username)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext {
                User = claimsPrincipal
            }
        };

        // Act
        var result = _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var userDto = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(username, userDto.Email);
        Assert.Equal(username, userDto.Username);
        Assert.Equal(username, userDto.Name);
    }

    [Fact]
    public void GetCurrentUser_WithUnauthenticatedUser_ReturnsUnauthorized() {
        // Arrange
        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        };

        // Act
        var result = _controller.GetCurrentUser();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var errorResponse = Assert.IsAssignableFrom<object>(unauthorizedResult.Value);
        var properties = errorResponse.GetType().GetProperties();
        var messageProperty = properties.FirstOrDefault(p => p.Name == "Message");
        Assert.NotNull(messageProperty);
        Assert.Equal("User not authenticated", messageProperty.GetValue(errorResponse));
    }

    [Fact]
    public void GetCurrentUser_WhenExceptionOccurs_ReturnsInternalServerError() {
        // Arrange
        _controller.ControllerContext = new ControllerContext {
            HttpContext = null! // This will cause a NullReferenceException
        };

        // Act
        var result = _controller.GetCurrentUser();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

        var errorResponse = Assert.IsAssignableFrom<object>(statusCodeResult.Value);
        var properties = errorResponse.GetType().GetProperties();

        var simpleMessageProperty = properties.FirstOrDefault(p => p.Name == "SimpleMessage");
        Assert.NotNull(simpleMessageProperty);
        Assert.Equal("An error occurred while retrieving user data",
            simpleMessageProperty.GetValue(errorResponse));

        var reasonProperty = properties.FirstOrDefault(p => p.Name == "Reason");
        Assert.NotNull(reasonProperty);
        Assert.NotNull(reasonProperty.GetValue(errorResponse));
    }
}
