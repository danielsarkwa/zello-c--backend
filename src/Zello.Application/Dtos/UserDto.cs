using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Dtos;

// User DTOs
/// <summary>
/// Data transfer object for reading user information
/// </summary>
/// <example>
/// {
///     "id": "123e4567-e89b-12d3-a456-426614174000",
///     "username": "johndoe",
///     "name": "John Doe",
///     "email": "john.doe@example.com",
///     "createdDate": "2024-01-01T12:00:00Z",
///     "accessLevel": "Member"
/// }
/// </example>
public class UserReadDto {
    /// <summary>
    /// Unique identifier of the user
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public Guid Id { get; set; }

    /// <summary>
    /// User's login username
    /// </summary>
    /// <example>johndoe</example>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's full name
    /// </summary>
    /// <example>John Doe</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    /// <example>john.doe@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Date when the user account was created
    /// </summary>
    /// <example>2024-01-01T12:00:00Z</example>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// User's system-wide access level
    /// </summary>
    /// <example>Member</example>
    public AccessLevel AccessLevel { get; set; }

    public static UserReadDto FromEntity(User user) {
        return new UserReadDto {
            Id = user.Id,
            Username = user.Username,
            Name = user.Name,
            Email = user.Email,
            CreatedDate = user.CreatedDate,
            AccessLevel = user.AccessLevel,
        };
    }
}

/// <summary>
/// Data transfer object for creating a new user
/// </summary>
/// <example>
/// {
///     "userName": "johndoe",
///     "name": "John Doe",
///     "email": "john.doe@example.com",
///     "password": "password123",
///     "accessLevel": "Member"
/// }
/// </example>
public class UserCreateDto {
    /// <summary>
    /// User's login username (3-20 characters)
    /// </summary>
    /// <example>johndoe</example>
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("userName")]
    public required string Username { get; set; }

    /// <summary>
    /// User's full name
    /// </summary>
    /// <example>John Doe</example>
    [Required]
    [JsonProperty("name")]
    public required string Name { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    /// <example>john.doe@example.com</example>
    [Required]
    [EmailAddress]
    [JsonProperty("email")]
    public required string Email { get; set; }

    /// <summary>
    /// User's password (minimum 8 characters)
    /// </summary>
    /// <example>password123</example>
    [Required]
    [StringLength(int.MaxValue, MinimumLength = 8)]
    [JsonProperty("password")]
    public required string Password { get; set; }

    /// <summary>
    /// Initial access level for the user
    /// </summary>
    /// <example>Member</example>
    [Required]
    [JsonProperty("accessLevel")]
    public required AccessLevel AccessLevel { get; set; }

    public User ToEntity() {
        return new User {
            Id = Guid.NewGuid(),
            Username = Username,
            Name = Name,
            Email = Email,
            PasswordHash = Password, // Since we're not hashing yet
            CreatedDate = DateTime.UtcNow,
            AccessLevel = AccessLevel.Member
        };
    }
}

/// <summary>
/// Data transfer object for updating user information
/// </summary>
/// <example>
/// {
///     "userName": "johndoe_updated",
///     "name": "John Doe Updated",
///     "email": "john.updated@example.com",
///     "accessLevel": "Member"
/// }
/// </example>
public class UserUpdateDto {
    /// <summary>
    /// Updated username
    /// </summary>
    /// <example>johndoe_updated</example>
    [JsonProperty("userName")]
    public string? Name { get; set; }

    /// <summary>
    /// Updated full name
    /// </summary>
    /// <example>John Doe Updated</example>
    [JsonProperty("name")]
    public string? Username { get; set; }

    /// <summary>
    /// Updated email address
    /// </summary>
    /// <example>john.updated@example.com</example>
    [JsonProperty("email")]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Updated access level
    /// </summary>
    /// <example>Member</example>
    [JsonProperty("accessLevel")]
    public AccessLevel AccessLevel { get; set; }

    public User ToEntity(User user) {
        user.Name = Name ?? user.Name;
        user.Username = Username ?? user.Username;
        user.Email = Email ?? user.Email;
        user.AccessLevel = AccessLevel;
        return user;
    }
}

/// <summary>
/// Data transfer object for changing user password
/// </summary>
/// <example>
/// {
///     "oldPassword": "oldpassword123",
///     "newPassword": "newpassword123"
/// }
/// </example>
public class ChangePasswordDto {
    /// <summary>
    /// User's current password
    /// </summary>
    /// <example>oldpassword123</example>
    [Required]
    [JsonProperty("oldPassword")]
    public required string OldPassword { get; set; }

    /// <summary>
    /// New password to set
    /// </summary>
    /// <example>newpassword123</example>
    [Required]
    [JsonProperty("newPassword")]
    public required string NewPassword { get; set; }
}
