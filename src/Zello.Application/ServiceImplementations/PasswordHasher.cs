using Zello.Application.ServiceInterfaces;
using Isopoh.Cryptography.Argon2;

namespace Zello.Application.ServiceImplementations;

public class PasswordHasher : IPasswordHasher {
    public string HashPassword(string password) {
        return Argon2.Hash(
            password,
            timeCost: 2,
            memoryCost: 65536,
            parallelism: 4,
            type: Argon2Type.DataIndependentAddressing,
            hashLength: 32);
    }

    public bool VerifyPassword(string password, string hash) {
        return Argon2.Verify(hash, password);
    }
}
