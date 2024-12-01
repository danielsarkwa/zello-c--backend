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
