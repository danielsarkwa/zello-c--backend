namespace Zello.Application.Common.Models.Authentication;

/// <summary>
/// Response model returned after successful authentication containing the JWT token and related information.
/// </summary>
/// <remarks>
/// This record encapsulates all the necessary information about the authentication token,
/// including its expiration time and the access level granted to the user.
/// </remarks>
public record LoginResponse {
    /// <summary>
    /// Gets or sets the JWT (JSON Web Token) string.
    /// This token should be included in the Authorization header for subsequent API requests.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Gets or sets the expiration date and time of the token.
    /// After this time, the token will no longer be valid and a new one must be obtained.
    /// </summary>
    public DateTime Expires { get; set; }

    /// <summary>
    /// Gets or sets the type of token, typically "Bearer" for JWT authentication.
    /// This value should be used as a prefix when sending the token in the Authorization header.
    /// </summary>
    public required string TokenType { get; set; }

    /// <summary>
    /// Gets or sets the string representation of the user's access level.
    /// This corresponds to the AccessLevel enum value (e.g., "Guest", "Member", "Owner", "Admin").
    /// </summary>
    public required string AccessLevel { get; set; }

    /// <summary>
    /// Gets or sets the numeric value of the access level.
    /// This value corresponds to the underlying integer value of the AccessLevel enum,
    /// useful for hierarchical access level comparisons.
    /// </summary>
    public int NumericLevel { get; set; }

    /// <summary>
    /// Gets or sets a human-readable description of the access level's privileges and capabilities.
    /// Provides context about what operations are available at this access level.
    /// </summary>
    public required string Description { get; set; }
}
