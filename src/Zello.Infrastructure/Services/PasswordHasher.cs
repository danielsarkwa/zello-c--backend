using Zello.Application.Interfaces;
using Isopoh.Cryptography.Argon2;

namespace Zello.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher {
    public string HashPassword(string password) {
        return Argon2.Hash(password);
    }

    public bool VerifyPassword(string password, string hash) {
        return Argon2.Verify(hash, password);
    }
}
