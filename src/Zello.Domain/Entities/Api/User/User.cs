using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Zello.Domain.Entities.Api.User;

public sealed class ApiUser {
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("username")]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    [JsonProperty("email")]
    public required string Email { get; set; }

    [Required]
    [StringLength(int.MaxValue, MinimumLength = 8)]
    [JsonProperty("password")]
    public required string Password { get; set; }

    [Required]
    [JsonProperty("name")]
    public required string Name { get; set; }
}
