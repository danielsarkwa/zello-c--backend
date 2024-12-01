using Zello.Application.Dtos;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;

namespace Zello.Application.ServiceInterfaces;

public interface IUserService {
    Task<UserReadDto> GetUserByIdAsync(Guid userId);
    Task<IEnumerable<UserReadDto>> GetAllUsersAsync();
    Task<UserReadDto> CreateUserAsync(UserCreateDto registerDto, IPasswordHasher passwordHasher);
    Task<UserReadDto> UpdateUserAsync(Guid userId, UserUpdateDto updateDto);
    Task DeleteUserAsync(Guid userId);
    Task<User?> GetUserByUsernameAsync(string username);
}
