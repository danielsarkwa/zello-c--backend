using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;

namespace Zello.Application.ServiceImplementations;

public class JwtTokenService : ITokenService {
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration) {
        _configuration = configuration;
    }

    public string GenerateToken(User user) {
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("AccessLevel", user.AccessLevel.ToString()),
            new Claim("UserId", user.Id.ToString())
        };

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey)) {
            throw new InvalidOperationException("JWT Key not configured");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
                _configuration["Jwt:Issuer"],
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
                 _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
