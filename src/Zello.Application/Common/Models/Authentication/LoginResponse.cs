namespace Zello.Application.Common.Models.Authentication;

public class LoginResponse {
    public required string Token { get; set; }
    public DateTime Expires { get; set; }
    public string TokenType { get; set; } = "Bearer";
}
