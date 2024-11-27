using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Zello.Application.Dtos;

public class LoginRequestDto {
    [Required]
    [StringLength(20, MinimumLength = 3)]
    [JsonProperty("username")]
    public required string UserName { get; set; }

    [Required]
    [JsonProperty("password")]
    public required string Password { get; set; }
}

public class LoginResponseDto {
    public required string Token { get; set; }
    public required string TokenType { get; set; }
    public DateTime Expires { get; set; }
    public required string AccessLevel { get; set; }
    public required int NumericLevel { get; set; } // reconsider this since the access level is now an Enum
    public required string Description { get; set; } // reconsider this
}

// not sure exactly what this is for, but it seems to be for refreshing the token
// maybe we need to think a bit more about this
public class TokenRequestDto {
    [Required]
    [JsonProperty("userName")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters in length")]
    public required string Username { get; set; }
}

// public class TokenResponseDto {
// }

// not sure if we will need this, for log out the request HEADER will be enough since it will have the authorization token
public class LogoutRequestDto {
    [JsonProperty("refreshToken")]
    public required string RefreshToken { get; set; }
}
