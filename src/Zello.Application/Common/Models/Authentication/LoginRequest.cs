using System.ComponentModel.DataAnnotations;

namespace Zello.Application.Common.Models.Authentication;

public class LoginRequest {
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
