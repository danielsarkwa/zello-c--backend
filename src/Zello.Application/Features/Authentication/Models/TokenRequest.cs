using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Application.Features.Authentication.Models;

public class TokenRequest {
    [Required]
    [JsonProperty("username")]
    public required string Username { get; set; }

    [Required]
    [JsonProperty("password")]
    public required string Password { get; set; }
}
