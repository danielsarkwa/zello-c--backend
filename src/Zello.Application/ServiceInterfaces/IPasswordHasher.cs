namespace Zello.Application.ServiceInterfaces;

public interface IPasswordHasher {
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
