using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zello.Application.Features.Authentication.Models;
using Zello.Application.Features.Users.Models;
using Zello.Application.Interfaces;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.Helpers;
using Zello.Infrastructure.TestingDataStorage;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase {
    private readonly IAuthenticationService _authService;
    private readonly IUserClaimsService _userClaimsService;
    private readonly IUserIdentityService _userIdentityService;
    private readonly IPasswordHasher _passwordHasher;

    public UserController(
        IAuthenticationService authService,
        IUserClaimsService userClaimsService,
        IUserIdentityService userIdentityService,
        IPasswordHasher passwordHasher) {
        _authService = authService;
        _userClaimsService = userClaimsService;
        _userIdentityService = userIdentityService;
        _passwordHasher = passwordHasher;
    }


    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] TokenRequest request) { // Remove async
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var loginResponse = _authService.AuthenticateUser(request.Username, request.Password);

        if (loginResponse == null) {
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        return Ok(loginResponse);
    }


    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser() {
        var userId = _userIdentityService.GetUserId(User);
        var username = _userIdentityService.GetUsername(User);
        var accessLevel = _userClaimsService.GetAccessLevel(User);

        if (userId == null || string.IsNullOrEmpty(username)) {
            return Unauthorized(new { Message = "User not authenticated" });
        }

        var userDto = new UserDto(
            id: userId.Value,
            username: username,
            email: username,
            name: username
        ) {
            AccessLevel = accessLevel ?? AccessLevel.Guest
        };

        return Ok(userDto);
    }


    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult RegisterUser([FromBody] RegisterDto registerDto) {
        Console.WriteLine($"Starting user registration for username: {registerDto.Username}");

        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        // Check if username already exists
        var existingUser = TestData.FindUserByUsername(registerDto.Username);
        if (existingUser != null) {
            return BadRequest(new { Message = "Username already exists" });
        }

        // Create new user
        var user = new UserDto(
            id: Guid.NewGuid(),
            username: registerDto.Username,
            email: registerDto.Email,
            name: registerDto.Name
        ) {
            AccessLevel = AccessLevel.Guest,
            PasswordHash = registerDto.Password, // Since we're not hashing yet
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        Console.WriteLine($"Adding user: {user.Username} with ID: {user.Id}");

        // Add user using the new method
        TestData.AddUser(user);

        // Verify the user was added
        var addedUser = TestData.FindUserByUsername(user.Username);
        if (addedUser == null) {
            Console.WriteLine("Failed to add user to collection");
            return StatusCode(500, new { Message = "Failed to create user" });
        }

        Console.WriteLine($"Successfully added user: {addedUser.Username} with ID: {addedUser.Id}");

        return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, user);
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUserById(Guid userId) {
        if (!TestData.TestUserCollection.TryGetValue(userId, out var user)) {
            return NotFound($"User with ID {userId} not found");
        }

        return Ok(user);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public IActionResult GetAllUsers() {
        return Ok(TestData.TestUserCollection.Values);
    }

    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateUser(Guid userId, [FromBody] RegisterDto updateDto) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        if (!TestData.TestUserCollection.ContainsKey(userId)) {
            return NotFound($"User with ID {userId} not found");
        }

        // Check if new username conflicts with existing users
        if (TestData.TestUserCollection.Values.Any(u =>
                u.Id != userId && u.Username == updateDto.Username)) {
            return BadRequest("Username already exists");
        }

        var existingUser = TestData.TestUserCollection[userId];
        var updatedUser = new UserDto(
            id: userId,
            username: updateDto.Username,
            email: updateDto.Email,
            name: updateDto.Name
        ) {
            AccessLevel = updateDto.AccessLevel, // Update access level
            PasswordHash = existingUser.PasswordHash,
            IsActive = existingUser.IsActive,
            CreatedDate = existingUser.CreatedDate,
            WorkspaceMembers = existingUser.WorkspaceMembers,
            AssignedTasks = existingUser.AssignedTasks,
            Comments = existingUser.Comments
        };

        TestData.TestUserCollection[userId] = updatedUser;

        return Ok(updatedUser);
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteUser(Guid userId) {
        if (!TestData.TestUserCollection.ContainsKey(userId)) {
            return NotFound($"User with ID {userId} not found");
        }

        TestData.TestUserCollection.Remove(userId);

        // Clean up related data
        var workspaceMembersToRemove = TestData.TestWorkspaceMemberCollection.Values
            .Where(wm => wm.UserId == userId)
            .Select(wm => wm.Id)
            .ToList();

        foreach (var memberId in workspaceMembersToRemove) {
            TestData.TestWorkspaceMemberCollection.Remove(memberId);
        }

        return NoContent();
    }
}
