// using System.Security.Claims;
// using Microsoft.AspNetCore.Mvc;
// using Zello.Domain.Entities.Dto;
// using Zello.Application.Features.Users.Models;
// using Zello.Domain.Entities.Api.User;
// using Zello.Application.Interfaces;
// using Zello.Application.Features.Authentication.Models;
// using Moq;
// using Zello.Api.Controllers;
//
// namespace Zello.Api.UnitTests;
//
// public class UserControllerTests {
//     private readonly UserController _controller;
//     private readonly Mock<IAuthenticationService> _mockAuthService;
//     private readonly Mock<IUserClaimsService> _mockUserClaimsService;
//     private readonly Mock<IUserIdentityService> _mockUserIdentityService;
//     private readonly Mock<IPasswordHasher> _mockPasswordHasher;
//
//     public UserControllerTests() {
//         _mockAuthService = new Mock<IAuthenticationService>();
//         _mockUserClaimsService = new Mock<IUserClaimsService>();
//         _mockUserIdentityService = new Mock<IUserIdentityService>();
//         _mockPasswordHasher = new Mock<IPasswordHasher>();
//
//         _controller = new UserController(
//             _mockAuthService.Object,
//             _mockUserClaimsService.Object,
//             _mockUserIdentityService.Object,
//             _mockPasswordHasher.Object
//         );
//     }
//
//     [Fact]
//     public void GetUserById_NonexistentId_ReturnsNotFound() {
//         // Arrange
//         var userId = Guid.NewGuid();
//
//         // Act
//         var result = _controller.GetUserById(userId);
//
//         // Assert
//         var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//         Assert.Contains(userId.ToString(), notFoundResult.Value?.ToString() ?? string.Empty);
//     }
//
//     [Fact]
//     public void GetAllUsers_ReturnsOkResult() {
//         // Act
//         var result = _controller.GetAllUsers();
//
//         // Assert
//         Assert.IsType<OkObjectResult>(result);
//     }
//
//     [Fact]
//     public void UpdateUser_NonexistentId_ReturnsNotFound() {
//         // Arrange
//         var userId = Guid.NewGuid();
//         var updateDto = new RegisterDto {
//             Username = "testuser",
//             Email = "test@test.com",
//             Name = "Test User",
//             AccessLevel = AccessLevel.Member,
//             Password = "password"
//         };
//
//         // Act
//         var result = _controller.UpdateUser(userId, updateDto);
//
//         // Assert
//         var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//         Assert.Contains(userId.ToString(), notFoundResult.Value?.ToString() ?? string.Empty);
//     }
//
//     [Fact]
//     public void DeleteUser_NonexistentId_ReturnsNotFound() {
//         // Arrange
//         var userId = Guid.NewGuid();
//
//         // Act
//         var result = _controller.DeleteUser(userId);
//
//         // Assert
//         var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//         Assert.Contains(userId.ToString(), notFoundResult.Value?.ToString() ?? string.Empty);
//     }
//
//     [Fact]
//     public void RegisterUser_ValidInput_ReturnsCreatedResult() {
//         // Arrange
//         var registerDto = new RegisterDto {
//             Username = "newuser",
//             Email = "new@test.com",
//             Name = "Test User",
//             AccessLevel = AccessLevel.Member,
//             Password = "password"
//         };
//
//         _mockPasswordHasher.Setup(x => x.HashPassword(It.IsAny<string>()))
//             .Returns("hashedPassword");
//
//         // Act
//         var result = _controller.RegisterUser(registerDto);
//
//         // Assert
//         var createdResult = Assert.IsType<CreatedAtActionResult>(result);
//         var user = Assert.IsType<UserDto>(createdResult.Value);
//         Assert.Equal(registerDto.Username, user.Username);
//         Assert.Equal(registerDto.Email, user.Email);
//         Assert.Equal(registerDto.Name, user.Name);
//     }
//
//     [Fact]
//     public void Login_ValidCredentials_ReturnsOkResult() {
//         // Arrange
//         var loginRequest = new TokenRequest {
//             Username = "testuser",
//             Password = "password"
//         };
//
//         var loginResponse = new LoginResponse {
//             Token = "jwt_token",
//             Expires = DateTime.Now.AddHours(1),
//             TokenType = "Bearer",
//             AccessLevel = "Member",
//             NumericLevel = 1,
//             Description = "Standard user access"
//         };
//
//         _mockAuthService.Setup(x => x.AuthenticateUser(It.IsAny<TokenRequest>()))
//             .Returns(loginResponse);
//
//         // Act
//         var result = _controller.Login(loginRequest);
//
//         // Assert
//         var okResult = Assert.IsType<OkObjectResult>(result);
//         var response = Assert.IsType<LoginResponse>(okResult.Value);
//         Assert.Equal(loginResponse.Token, response.Token);
//     }
//
//     [Fact]
//     public void Login_InvalidCredentials_ReturnsUnauthorized() {
//         // Arrange
//         var loginRequest = new TokenRequest {
//             Username = "invalid",
//             Password = "invalid"
//         };
//
//         _mockAuthService.Setup(x => x.AuthenticateUser(It.IsAny<TokenRequest>()))
//             .Returns((LoginResponse?)null);
//
//         // Act
//         var result = _controller.Login(loginRequest);
//
//         // Assert
//         Assert.IsType<UnauthorizedObjectResult>(result);
//     }
//
//     [Theory]
//     [InlineData(null)]
//     [InlineData("")]
//     [InlineData(" ")]
//     public void RegisterUser_InvalidEmail_ReturnsBadRequest(string? email) {
//         // Arrange
//         var registerDto = new RegisterDto {
//             Username = "testuser",
//             Email = email,
//             Name = "Test User",
//             AccessLevel = AccessLevel.Member,
//             Password = "password"
//         };
//
//         // Validate and add model error
//         _controller.ModelState.AddModelError("Email", "Email is required");
//
//         // Act
//         var result = _controller.RegisterUser(registerDto);
//
//         // Assert
//         Assert.IsType<BadRequestObjectResult>(result);
//     }
//
//     [Theory]
//     [InlineData("", "Valid Name", "valid@email.com")]
//     [InlineData("validuser", "", "valid@email.com")]
//     [InlineData("validuser", "Valid Name", "")]
//     public void UpdateUser_InvalidFields_ReturnsBadRequest(string username, string name,
//         string email) {
//         // Arrange
//         var userId = Guid.NewGuid();
//         var updateDto = new RegisterDto {
//             Username = username,
//             Name = name,
//             Email = email,
//             AccessLevel = AccessLevel.Member,
//             Password = "password"
//         };
//
//         // Validate and add model error
//         _controller.ModelState.AddModelError("Input", "Required fields missing");
//
//         // Act
//         var result = _controller.UpdateUser(userId, updateDto);
//
//         // Assert
//         Assert.IsType<BadRequestObjectResult>(result);
//     }
//
//     [Fact]
//     public void GetCurrentUser_AuthenticatedUser_ReturnsOkResult() {
//         // Arrange
//         var userId = Guid.NewGuid();
//         var username = "testuser";
//         var accessLevel = AccessLevel.Member;
//
//         _mockUserIdentityService.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
//             .Returns(userId);
//         _mockUserIdentityService.Setup(x => x.GetUsername(It.IsAny<ClaimsPrincipal>()))
//             .Returns(username);
//         _mockUserClaimsService.Setup(x => x.GetAccessLevel(It.IsAny<ClaimsPrincipal>()))
//             .Returns(accessLevel);
//
//         // Act
//         var result = _controller.GetCurrentUser();
//
//         // Assert
//         var okResult = Assert.IsType<OkObjectResult>(result);
//         var user = Assert.IsType<UserDto>(okResult.Value);
//         Assert.Equal(userId, user.Id);
//         Assert.Equal(username, user.Username);
//         Assert.Equal(accessLevel, user.AccessLevel);
//     }
//
//     [Fact]
//     public void GetCurrentUser_UnauthenticatedUser_ReturnsUnauthorized() {
//         // Arrange
//         _mockUserIdentityService.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>()))
//             .Returns((Guid?)null);
//         _mockUserIdentityService.Setup(x => x.GetUsername(It.IsAny<ClaimsPrincipal>()))
//             .Returns((string?)null);
//
//         // Act
//         var result = _controller.GetCurrentUser();
//
//         // Assert
//         Assert.IsType<UnauthorizedObjectResult>(result);
//     }
// }
