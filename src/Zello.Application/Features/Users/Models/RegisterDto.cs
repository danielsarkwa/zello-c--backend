
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Features.Users.Models;

public record RegisterDto {
    [Required]
    [JsonProperty("username")]
    public required string Username { get; set; }

    [Required]
    [JsonProperty("email")]
    public required string Email { get; set; }

    [Required]
    [JsonProperty("name")]
    public required string Name { get; set; }

    [Required]
    [MinLength(6)]
    [JsonProperty("password")]
    public required string Password { get; set; }

    [Required]
    [JsonProperty("accessLevel")]
    public AccessLevel AccessLevel { get; set; } = AccessLevel.Guest;
}
