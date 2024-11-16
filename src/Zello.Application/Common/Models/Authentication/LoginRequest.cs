using System.ComponentModel.DataAnnotations;

namespace Zello.Application.Common.Models.Authentication;

public class LoginRequest {
    public required string Username { get; set; }
    public required string Password { get; set; }
}
