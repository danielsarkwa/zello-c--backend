using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Zello.Application.Common.Models.Authentication;
using Zello.Domain.Entities.Api.User;

namespace Zello.Api.Controllers.Test;

#if DEBUG
[ApiController]
[Route("api/v1/test/retrieve-jwt")]
[ApiExplorerSettings(GroupName = "Testing")] // Separate Swagger group for test endpoints
public class RetrieveElevatedJwt : ControllerBase {
    private readonly IConfiguration _configuration;

    public RetrieveElevatedJwt(IConfiguration configuration) {
        _configuration = configuration;
    }

    [HttpGet("available-levels")]
    public IActionResult GetAvailableLevels() {
        var levels = Enum.GetValues<AccessLevel>()
            .Select(level => new {
                Name = level.ToString(),
                Level = (int)level,
                Description = GetAccessLevelDescription(level)
            });

        return Ok(new {
            AvailableLevels = levels,
            Usage = new {
                GetToken = "POST /api/v1/test/accessjwt/token/{level}",
                SpecificEndpoints =
                    "POST /api/v1/test/accessjwt/{level} (guest/member/owner/admin)",
                TestAccess = "Use token with /api/v1/accesstest/* endpoints"
            }
        });
    }

    [HttpPost("token/{level}")]
    public IActionResult GetTokenForLevel(string level, [FromBody] TokenRequest request) {
        if (string.IsNullOrEmpty(request.Username)) {
            return BadRequest(new { Message = "Username is required" });
        }

        if (!Enum.TryParse<AccessLevel>(level, true, out var accessLevel)) {
            return BadRequest(new {
                Message =
                    $"Invalid access level. Valid levels are: {string.Join(", ", Enum.GetNames<AccessLevel>())}",
                AvailableLevels = Enum.GetValues<AccessLevel>().Select(l => new {
                    Name = l.ToString(),
                    Level = (int)l,
                    Description = GetAccessLevelDescription(l)
                })
            });
        }

        var token = GenerateJwtToken(request.Username, accessLevel);
        var response = new LoginResponse {
            Token = token,
            Expires = DateTime.Now.AddHours(1),
            TokenType = "Bearer",
            AccessLevel = accessLevel.ToString(),
            NumericLevel = (int)accessLevel,
            Description = GetAccessLevelDescription(accessLevel)
        };

        return Ok(response);
    }

    // Convenience endpoints for each access level
    [HttpPost("guest")]
    public IActionResult GetGuestToken([FromBody] TokenRequest request) =>
        GetTokenForLevel("Guest", request);

    [HttpPost("member")]
    public IActionResult GetMemberToken([FromBody] TokenRequest request) =>
        GetTokenForLevel("Member", request);

    [HttpPost("owner")]
    public IActionResult GetOwnerToken([FromBody] TokenRequest request) =>
        GetTokenForLevel("Owner", request);

    [HttpPost("admin")]
    public IActionResult GetAdminToken([FromBody] TokenRequest request) =>
        GetTokenForLevel("Admin", request);

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

    private string GetAccessLevelDescription(AccessLevel level) => level switch {
        AccessLevel.Guest => "Basic access with limited privileges",
        AccessLevel.Member => "Standard user access with basic features",
        AccessLevel.Owner => "Elevated access with management capabilities",
        AccessLevel.Admin => "Full system access with all privileges",
        _ => "Unknown access level"
    };
}
#endif
