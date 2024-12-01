using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Application.Features.Authentication.Models;

/// <summary>
/// Data transfer object for user authentication requests
/// </summary>
/// <example>
/// {
///     "username": "johndoe",
///     "password": "password123"
/// }
/// </example>
public class TokenRequest {
    /// <summary>
    /// Username for authentication
    /// </summary>
    /// <example>johndoe</example>
    [Required]
    [JsonProperty("username")]
    public required string Username { get; set; }

    /// <summary>
    /// User's password
    /// </summary>
    /// <example>password123</example>
    [Required]
    [JsonProperty("password")]
    public required string Password { get; set; }
}
