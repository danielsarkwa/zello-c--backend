using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zello.Application.Features.Authentication.Models;
using Zello.Application.Features.Users.Models;
using Zello.Application.Interfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Helpers;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UserController : ControllerBase {
    private readonly ApplicationDbContext _context;
    private readonly IAuthenticationService _authService;
    private readonly IUserClaimsService _userClaimsService;
    private readonly IUserIdentityService _userIdentityService;
    private readonly IPasswordHasher _passwordHasher;

    public UserController(
        ApplicationDbContext context,
        IAuthenticationService authService,
        IUserClaimsService userClaimsService,
        IUserIdentityService userIdentityService,
        IPasswordHasher passwordHasher) {
        _context = context;
        _authService = authService;
        _userClaimsService = userClaimsService;
        _userIdentityService = userIdentityService;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] TokenRequest request) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var loginResponse = await _authService.AuthenticateUserAsync(request);

        if (loginResponse == null)
            return Unauthorized(new { Message = "Invalid credentials" });

        return Ok(loginResponse);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser() {
        var userId = _userIdentityService.GetUserId(User);
        if (userId == null)
            return Unauthorized(new { Message = "User not authenticated" });

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null)
            return NotFound(new { Message = "User not found" });

        var accessLevel = _userClaimsService.GetAccessLevel(User);

        var userDto = new UserDto(
            id: user.Id,
            username: user.Username,
            email: user.Email,
            name: user.Name
        ) {
            AccessLevel = accessLevel ?? AccessLevel.Guest
        };

        return Ok(userDto);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterDto registerDto) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if username already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == registerDto.Username);

        if (existingUser != null)
            return BadRequest(new { Message = "Username already exists" });

        var hashedPassword = _passwordHasher.HashPassword(registerDto.Password);

        // Create new user
        var user = new User {
            Id = Guid.NewGuid(),
            Username = registerDto.Username,
            Email = registerDto.Email,
            Name = registerDto.Name,
            AccessLevel = AccessLevel.Guest,
            PasswordHash = hashedPassword,
            CreatedDate = DateTime.UtcNow
        };

        try {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDto(
                id: user.Id,
                username: user.Username,
                email: user.Email,
                name: user.Name
            ) {
                AccessLevel = user.AccessLevel,
                CreatedDate = user.CreatedDate
            };

            return CreatedAtAction(nameof(GetUserById), new { userId = user.Id }, userDto);
        } catch (Exception ex) {
            return StatusCode(500, new { Message = "Failed to create user", Error = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid userId) {
        var user = await _context.Users
            .Include(u => u.WorkspaceMembers)
            .Include(u => u.AssignedTasks)
            .Include(u => u.Comments)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound($"User with ID {userId} not found");

        // Create new User instance with all required properties
        var userDto = new User {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Name = user.Name,
            PasswordHash = user.PasswordHash,
            AccessLevel = user.AccessLevel,
            CreatedDate = user.CreatedDate,
            WorkspaceMembers = user.WorkspaceMembers?.ToList() ?? new List<WorkspaceMember>(),
            AssignedTasks = user.AssignedTasks?.ToList() ?? new List<TaskAssignee>(),
            Comments = user.Comments?.ToList() ?? new List<Comment>()
        };

        return Ok(userDto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers() {
        var users = await _context.Users
            .Include(u => u.WorkspaceMembers)
            .Include(u => u.AssignedTasks)
            .Include(u => u.Comments)
            .ToListAsync();

        var userDtos = users.Select(user => new User {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Name = user.Name,
            PasswordHash = user.PasswordHash,
            AccessLevel = user.AccessLevel,
            CreatedDate = user.CreatedDate,
            WorkspaceMembers = user.WorkspaceMembers?.ToList() ?? new List<WorkspaceMember>(),
            AssignedTasks = user.AssignedTasks?.ToList() ?? new List<TaskAssignee>(),
            Comments = user.Comments?.ToList() ?? new List<Comment>()
        }).ToList();

        return Ok(userDtos);
    }

    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] RegisterDto updateDto) {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _context.Users
            .Include(u => u.WorkspaceMembers)
            .Include(u => u.AssignedTasks)
            .Include(u => u.Comments)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound($"User with ID {userId} not found");

        // Check if new username conflicts with existing users
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id != userId && u.Username == updateDto.Username);

        if (existingUser != null)
            return BadRequest("Username already exists");

        // Update user properties
        user.Username = updateDto.Username;
        user.Email = updateDto.Email;
        user.Name = updateDto.Name;
        user.AccessLevel = updateDto.AccessLevel;

        try {
            await _context.SaveChangesAsync();

            var userDto = new User {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Name = user.Name,
                PasswordHash = user.PasswordHash,
                AccessLevel = user.AccessLevel,
                CreatedDate = user.CreatedDate,
                WorkspaceMembers = user.WorkspaceMembers?.ToList() ?? new List<WorkspaceMember>(),
                AssignedTasks = user.AssignedTasks?.ToList() ?? new List<TaskAssignee>(),
                Comments = user.Comments?.ToList() ?? new List<Comment>()
            };

            return Ok(userDto);
        } catch (Exception ex) {
            return StatusCode(500, new { Message = "Failed to update user", Error = ex.Message });
        }
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid userId) {
        var user = await _context.Users
            .Include(u => u.WorkspaceMembers)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound($"User with ID {userId} not found");

        try {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        } catch (Exception ex) {
            return StatusCode(500, new { Message = "Failed to delete user", Error = ex.Message });
        }
    }
}
