using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.Dtos;

public class UserReadDto {
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
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

public class UserCreateDto {
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("userName")]
    public required string Username { get; set; }

    [Required]
    [JsonProperty("name")]
    public required string Name { get; set; }

    [Required]
    [EmailAddress]
    [JsonProperty("email")]
    public required string Email { get; set; }

    [Required]
    [StringLength(int.MaxValue, MinimumLength = 8)]
    [JsonProperty("password")]
    public required string Password { get; set; }

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

public class UserUpdateDTO {
    [JsonProperty("userName")]
    public string? Name { get; set; }

    [JsonProperty("name")]
    public string? Username { get; set; }

    [JsonProperty("email")]
    [EmailAddress]
    public string? Email { get; set; }

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

public class ChangePasswordDto {
    [Required]
    [JsonProperty("oldPassword")]
    public required string OldPassword { get; set; }

    [Required]
    [JsonProperty("newPassword")]
    public required string NewPassword { get; set; }
}
