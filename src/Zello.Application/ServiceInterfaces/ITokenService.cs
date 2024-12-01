using Zello.Domain.Entities;

namespace Zello.Application.ServiceInterfaces;

public interface ITokenService {
    string GenerateToken(User user);
}
