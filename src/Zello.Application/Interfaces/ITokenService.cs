using Zello.Domain.Entities.Dto;

namespace Zello.Infrastructure.Interfaces;

public interface ITokenService {
    string GenerateToken(UserDto user);
}
