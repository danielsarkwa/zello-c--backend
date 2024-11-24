using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Zello.Application.Features.Authentication.Models;
using Zello.Domain.Entities.Api.Message;
using Zello.Domain.Entities.Api.User;

namespace Zello.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase {
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration) {
        _configuration = configuration;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult Register([FromBody] RegisterUserRequest userRequest) {
        if (!ModelState.IsValid) {
            return ValidationProblem(ModelState);
        }

        // Here you would typically handle user registration
        // For now, just return success
        return Ok(new SimpleMessage { Message = "User registered successfully" });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request) {
        // For testing, accept any non-empty credentials
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password)) {
            return BadRequest(new { Message = "Username and password are required" });
        }
        // Later:
        // 1. Verify the username/password against the database
        // 2. Retrieve the user's access level from the database
        var accessLevel = AccessLevel.Member; // This would come from your user database

        var token = GenerateJwtToken(request.Username, accessLevel);
        var response = new LoginResponse {
            Token = token,
            Expires = DateTime.Now.AddHours(1),
            TokenType = "Bearer",
            AccessLevel = accessLevel.ToString(),
            Description = "not_set"
        };

        return Ok(response);
    }

    [HttpPost("logout")]
    [ProducesResponseType(typeof(SimpleMessage), StatusCodes.Status200OK)]
    public IActionResult Logout() {
        // JWTs cannot themselves be invalidated, maybe blacklist tokens later?
        return Ok(new SimpleMessage { Message = "Logged out successfully" });
    }

    private string GenerateJwtToken(string username, AccessLevel accessLevel) {
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim("AccessLevel", accessLevel.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ??
            throw new InvalidOperationException("JWT Key not configured")));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
