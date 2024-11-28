using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Domain.Entities.Dto;
using Zello.Application.Features.Users.Models;
using Zello.Domain.Entities.Api.User;
using Zello.Application.Interfaces;
using Zello.Application.Features.Authentication.Models;
using Zello.Domain.Entities;
using Zello.Infrastructure.Data;
using Moq;
using Zello.Api.Controllers;

namespace Zello.Api.UnitTests;

public class UserControllerTests : IDisposable {
    private readonly UserController _controller;
    private readonly ApplicationDbContext _context;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<IUserClaimsService> _mockUserClaimsService;
    private readonly Mock<IUserIdentityService> _mockUserIdentityService;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;

    public UserControllerTests() {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _mockAuthService = new Mock<IAuthenticationService>();
        _mockUserClaimsService = new Mock<IUserClaimsService>();
        _mockUserIdentityService = new Mock<IUserIdentityService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();

        _controller = new UserController(
            _context,
            _mockAuthService.Object,
            _mockUserClaimsService.Object,
            _mockUserIdentityService.Object,
            _mockPasswordHasher.Object
        );
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetUserById_NonexistentId_ReturnsNotFound() {
        var userId = Guid.NewGuid();
        var result = await _controller.GetUserById(userId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(userId.ToString(), notFoundResult.Value?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOkResult() {
        // Arrange
        var user = new User {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@test.com",
            Name = "Test User",
            AccessLevel = AccessLevel.Member,
            PasswordHash = "hash",
            CreatedDate = DateTime.UtcNow,
            WorkspaceMembers = new List<WorkspaceMember>(),
            AssignedTasks = new List<TaskAssignee>(),
            Comments = new List<Comment>()
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsType<List<User>>(okResult.Value);
        var returnedUser = Assert.Single(users);
        Assert.Equal(user.Username, returnedUser.Username);
        Assert.Equal(user.Email, returnedUser.Email);
        Assert.Equal(user.AccessLevel, returnedUser.AccessLevel);
    }

    [Fact]
    public async Task RegisterUser_ValidInput_ReturnsCreatedResult() {
        var registerDto = new RegisterDto {
            Username = "newuser",
            Email = "new@test.com",
            Name = "Test User",
            AccessLevel = AccessLevel.Member,
            Password = "password"
        };

        _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashedPassword");

        var result = await _controller.RegisterUser(registerDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var userDto = Assert.IsType<UserDto>(createdResult.Value);
        Assert.Equal(registerDto.Username, userDto.Username);
        Assert.Equal(registerDto.Email, userDto.Email);
        Assert.Equal(registerDto.Name, userDto.Name);
        Assert.True(userDto.IsActive);
        Assert.Equal(AccessLevel.Guest, userDto.AccessLevel);
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
            Description = "Standard user access"
        };

        _mockAuthService.Setup(x => x.AuthenticateUserAsync(It.IsAny<TokenRequest>()))
            .ReturnsAsync(loginResponse);

        var result = await _controller.Login(loginRequest);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal(loginResponse.Token, response.Token);
        Assert.Equal(loginResponse.AccessLevel, response.AccessLevel);
    }

    [Fact]
    public async Task GetCurrentUser_AuthenticatedUser_ReturnsOkResult() {
        var userId = Guid.NewGuid();
        var user = new User {
            Id = userId,
            Username = "testuser",
            Email = "test@test.com",
            Name = "Test User",
            AccessLevel = AccessLevel.Member,
            PasswordHash = "hash",
            CreatedDate = DateTime.UtcNow
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _mockUserIdentityService.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(userId);
        _mockUserClaimsService.Setup(x => x.GetAccessLevel(It.IsAny<ClaimsPrincipal>()))
            .Returns(AccessLevel.Member);

        var result = await _controller.GetCurrentUser();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var userDto = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(userId, userDto.Id);
        Assert.Equal(user.Username, userDto.Username);
        Assert.Equal(AccessLevel.Member, userDto.AccessLevel);
        Assert.True(userDto.IsActive);
    }

    [Theory]
    [InlineData("", "Valid Name", "valid@email.com")]
    [InlineData("validuser", "", "valid@email.com")]
    [InlineData("validuser", "Valid Name", "")]
    public async Task UpdateUser_InvalidFields_ReturnsBadRequest(string username, string name,
        string email) {
        var userId = Guid.NewGuid();
        var updateDto = new RegisterDto {
            Username = username,
            Name = name,
            Email = email,
            AccessLevel = AccessLevel.Member,
            Password = "password"
        };

        _controller.ModelState.AddModelError("Input", "Required fields missing");

        var result = await _controller.UpdateUser(userId, updateDto);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
