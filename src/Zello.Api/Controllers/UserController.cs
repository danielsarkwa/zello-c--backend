using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zello.Application.Dtos;
using Zello.Application.Features.Authentication.Models;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities.Api.User;
using Zello.Infrastructure.Helpers;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase {
    private readonly IUserService _userService;
    private readonly IAuthenticationService _authService;
    private readonly IPasswordHasher _passwordHasher;

    public UserController(
        IUserService userService,
        IAuthenticationService authService,
        IPasswordHasher passwordHasher) {
        _userService = userService;
        _authService = authService;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/User/login
    ///     {
    ///         "username": "johndoe",
    ///         "password": "password123"
    ///     }
    /// </remarks>
    /// <response code="200">Authentication successful</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] TokenRequest request) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var loginResponse = await _authService.AuthenticateUserAsync(request);
        return loginResponse == null
            ? Unauthorized(new { Message = "Invalid credentials" })
            : Ok(loginResponse);
    }

    /// <summary>
    /// Retrieves the current authenticated user's information
    /// </summary>
    /// <remarks>
    /// Requires authentication via JWT token
    /// </remarks>
    /// <response code="200">Returns the current user's information</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser() {
        var userId = ClaimsHelper.GetUserId(User);
        if (userId == null) return BadRequest("User ID missing");

        try {
            var userDto = await _userService.GetUserByIdAsync(userId.Value);
            return Ok(userDto);
        } catch (KeyNotFoundException) {
            return NotFound(new { Message = "User not found" });
        }
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="registerDto">User registration details</param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/v1/User/register
    ///     {
    ///         "userName": "johndoe",
    ///         "name": "John Doe",
    ///         "email": "john.doe@example.com",
    ///         "password": "password123",
    ///         "accessLevel": "Member"
    ///     }
    /// </remarks>
    /// <response code="201">User successfully registered</response>
    /// <response code="400">Invalid registration data or username already exists</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser([FromBody] UserCreateDto registerDto) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try {
            var userDto = await _userService.CreateUserAsync(registerDto, _passwordHasher);
            return CreatedAtAction(nameof(GetUserById), new { userId = userDto.Id }, userDto);
        } catch (InvalidOperationException ex) {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves a user by their ID
    /// </summary>
    /// <param name="userId">The unique identifier of the user</param>
    /// <remarks>
    /// Users can only view their own profile unless they have Admin access
    /// </remarks>
    /// <response code="200">Returns the requested user's information</response>
    /// <response code="403">Insufficient permissions to view user</response>
    /// <response code="404">User not found</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserById(Guid userId) {
        var currentUserId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);

        if (currentUserId == null) return BadRequest("User ID missing");
        if (userId != currentUserId && userAccess != AccessLevel.Admin)
            return Forbid("You can only view your own profile");

        try {
            var userDto = await _userService.GetUserByIdAsync(userId);
            return Ok(userDto);
        } catch (KeyNotFoundException) {
            return NotFound($"User with ID {userId} not found");
        }
    }

    /// <summary>
    /// Retrieves all users in the system
    /// </summary>
    /// <remarks>
    /// Required permissions:
    /// - Admin access level
    /// </remarks>
    /// <response code="200">Returns list of all users</response>
    /// <response code="403">User is not an administrator</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers() {
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);
        var userId = ClaimsHelper.GetUserId(User);

        if (userId == null) return BadRequest("User ID missing");
        if (userAccess != AccessLevel.Admin)
            return Forbid("Only administrators can view all users");

        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Updates a user's information
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update</param>
    /// <param name="updateDto">Updated user information</param>
    /// <remarks>
    /// Users can only update their own profile unless they have Admin access
    /// </remarks>
    /// <response code="200">User successfully updated</response>
    /// <response code="400">Invalid update data</response>
    /// <response code="403">Insufficient permissions to update user</response>
    /// <response code="404">User not found</response>
    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(UserReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UserUpdateDto updateDto) {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var currentUserId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);

        if (currentUserId == null) return BadRequest("User ID missing");
        if (userId != currentUserId && userAccess != AccessLevel.Admin)
            return Forbid("You can only update your own profile");

        try {
            var userDto = await _userService.UpdateUserAsync(userId, updateDto);
            return Ok(userDto);
        } catch (KeyNotFoundException) {
            return NotFound($"User with ID {userId} not found");
        } catch (InvalidOperationException ex) {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a user account
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete</param>
    /// <remarks>
    /// Users can only delete their own account unless they have Admin access
    /// </remarks>
    /// <response code="204">User successfully deleted</response>
    /// <response code="403">Insufficient permissions to delete user</response>
    /// <response code="404">User not found</response>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(Guid userId) {
        var currentUserId = ClaimsHelper.GetUserId(User);
        var userAccess = ClaimsHelper.GetUserAccessLevel(User);

        if (currentUserId == null) return BadRequest("User ID missing");
        if (userId != currentUserId && userAccess != AccessLevel.Admin)
            return Forbid("You can only delete your own profile");

        try {
            await _userService.DeleteUserAsync(userId);
            return NoContent();
        } catch (KeyNotFoundException) {
            return NotFound($"User with ID {userId} not found");
        }
    }
}
