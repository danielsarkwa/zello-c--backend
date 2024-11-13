using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Domain.Entities.Api.User;

public record UserDto {
    [Required]
    [JsonProperty("username")]
    public required string Username { get; set; }

    [Required]
    [JsonProperty("email")]
    public required string Email { get; set; }

    [Required]
    [JsonProperty("name")]
    public required string Name { get; set; }
}
