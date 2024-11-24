using Zello.Application.Interfaces;

namespace Zello.Infrastructure.Services;


// TO-DO hash passwords later
public class PasswordHasher : IPasswordHasher {
    public string HashPassword(string password) {
        return (password);
    }

    public bool VerifyPassword(string password, string hash) {
        return (password == hash);
    }
}
