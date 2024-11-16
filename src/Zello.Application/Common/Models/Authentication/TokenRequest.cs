using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Application.Common.Models.Authentication;

/// <summary>
/// Request model for obtaining authentication tokens, particularly useful in testing scenarios.
/// </summary>
/// <remarks>
/// This model is used primarily with the test endpoints to generate tokens with specific access levels.
/// The username is used to identify the token holder in the JWT claims.
/// </remarks>
public record TokenRequest {
    /// <summary>
    /// Gets or sets the username for the token request.
    /// </summary>
    /// <remarks>
    /// - Must be between 3 and 20 characters in length
    /// - Cannot be null or empty (Required attribute)
    /// - Will be serialized as "username" in JSON (JsonProperty attribute)
    /// </remarks>
    [Required]
    [StringLength(20, MinimumLength = 3,
        ErrorMessage = "Username must be between 3 and 20 characters in length")]
    [JsonProperty("username")]
    public required string Username { get; set; }
}
