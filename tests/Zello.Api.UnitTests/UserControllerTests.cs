using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Zello.Api.Controllers;
using Zello.Application.Dtos;
using Zello.Application.Features.Authentication.Models;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Api.UnitTests;

public class UserControllerTests {
    private readonly UserController _controller;
    private readonly Mock<IUserService> _userService;
    private readonly Mock<IAuthenticationService> _authService;
    private readonly Mock<IPasswordHasher> _passwordHasher;

    public UserControllerTests() {
        _userService = new Mock<IUserService>();
        _authService = new Mock<IAuthenticationService>();
        _passwordHasher = new Mock<IPasswordHasher>();

        _controller = new UserController(
            _userService.Object,
            _authService.Object,
            _passwordHasher.Object
        );
    }

    [Fact]
    public async Task GetUserById_NonexistentId_ReturnsNotFound() {
        var userId = Guid.NewGuid();
        _userService.Setup(x => x.GetUserByIdAsync(userId))
            .ThrowsAsync(new KeyNotFoundException());

        SetupUserContext(userId);
        var result = await _controller.GetUserById(userId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(userId.ToString(), notFoundResult.Value?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task GetUserById_DifferentUserNotAdmin_ReturnsForbidden() {
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        SetupUserContext(differentUserId);
        var result = await _controller.GetUserById(userId);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAllUsers_NonAdmin_ReturnsForbidden() {
        SetupUserContext(Guid.NewGuid());
        var result = await _controller.GetAllUsers();

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task GetAllUsers_AsAdmin_ReturnsOkResult() {
        var users = new List<UserReadDto> {
            new() {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@test.com",
                Name = "Test User",
                AccessLevel = AccessLevel.Member,
                CreatedDate = DateTime.UtcNow
            }
        };
        _userService.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(users);

        SetupUserContext(Guid.NewGuid(), isAdmin: true);
        var result = await _controller.GetAllUsers();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsType<List<UserReadDto>>(okResult.Value);
        var returnedUser = Assert.Single(returnedUsers);
        Assert.Equal(users[0].Username, returnedUser.Username);
    }

    [Fact]
    public async Task RegisterUser_ValidInput_ReturnsCreatedResult() {
        var registerDto = new UserCreateDto {
            Username = "newuser",
            Email = "new@test.com",
            Name = "Test User",
            AccessLevel = AccessLevel.Member,
            Password = "password"
        };
        var createdUser = UserReadDto.FromEntity(registerDto.ToEntity());

        _userService.Setup(x => x.CreateUserAsync(registerDto, _passwordHasher.Object))
            .ReturnsAsync(createdUser);

        var result = await _controller.RegisterUser(registerDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var userDto = Assert.IsType<UserReadDto>(createdResult.Value);
        Assert.Equal(registerDto.Username, userDto.Username);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkResult() {
        var loginRequest = new TokenRequest {
            Username = "testuser",
            Password = "password"
        };
        var loginResponse = new LoginResponse {
            Token = "jwt_token",
            Expires = DateTime.Now.AddHours(1),
            TokenType = "Bearer",
            AccessLevel = "Member",
            NumericLevel = 1,
            Description = "Standard user access with basic features"
        };

        _authService.Setup(x => x.AuthenticateUserAsync(loginRequest))
            .ReturnsAsync(loginResponse);

        var result = await _controller.Login(loginRequest);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal(loginResponse.Token, response.Token);
        Assert.Equal(loginResponse.Description, response.Description);
    }

    [Fact]
    public async Task GetCurrentUser_AuthenticatedUser_ReturnsOkResult() {
        var userId = Guid.NewGuid();
        var user = new UserReadDto {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            Name = "Test User",
            AccessLevel = AccessLevel.Member,
            CreatedDate = DateTime.UtcNow
        };

        _userService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        SetupUserContext(userId);
        var result = await _controller.GetCurrentUser();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var userDto = Assert.IsType<UserReadDto>(okResult.Value);
        Assert.Equal(userId, userDto.Id);
    }

    [Fact]
    public async Task UpdateUser_ValidInput_ReturnsOkResult() {
        var userId = Guid.NewGuid();
        var updateDto = new UserUpdateDto {
            Username = "updateduser",
            Name = "Updated Name",
            Email = "updated@test.com",
            AccessLevel = AccessLevel.Member
        };
        var updatedUser = new UserReadDto {
            Id = userId,
            Username = updateDto.Username!,
            Name = updateDto.Name!,
            Email = updateDto.Email!,
            AccessLevel = updateDto.AccessLevel,
            CreatedDate = DateTime.UtcNow
        };

        _userService.Setup(x => x.UpdateUserAsync(userId, updateDto))
            .ReturnsAsync(updatedUser);

        SetupUserContext(userId);
        var result = await _controller.UpdateUser(userId, updateDto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var userDto = Assert.IsType<UserReadDto>(okResult.Value);
        Assert.Equal(updateDto.Username, userDto.Username);
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_ReturnsNoContent() {
        var userId = Guid.NewGuid();
        SetupUserContext(userId);

        var result = await _controller.DeleteUser(userId);

        Assert.IsType<NoContentResult>(result);
        _userService.Verify(x => x.DeleteUserAsync(userId), Times.Once);
    }

    private void SetupUserContext(Guid userId, bool isAdmin = false) {
        var claims = new List<Claim> {
            new("UserId", userId.ToString()),
            new("AccessLevel",
                isAdmin ? AccessLevel.Admin.ToString() : AccessLevel.Member.ToString())
        };
        _controller.ControllerContext = new ControllerContext {
            HttpContext = new DefaultHttpContext {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };
    }
}
