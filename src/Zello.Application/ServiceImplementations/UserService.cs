using Microsoft.EntityFrameworkCore;
using Zello.Application.Dtos;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.RepositoryInterfaces;

namespace Zello.Application.ServiceImplementations;

public class UserService : IUserService {
    private readonly IUserRepository _userRepository;
    private readonly DbContext _context;

    public UserService(IUserRepository userRepository, DbContext context) {
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<UserReadDto> GetUserByIdAsync(Guid userId) {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        return UserReadDto.FromEntity(user);
    }

    public async Task<IEnumerable<UserReadDto>> GetAllUsersAsync() {
        var users = await _userRepository.GetAllAsync();
        return users.Select(UserReadDto.FromEntity);
    }

    public async Task<UserReadDto> CreateUserAsync(UserCreateDto registerDto,
        IPasswordHasher passwordHasher) {
        var existingUser = await _userRepository.GetUserByUsernameAsync(registerDto.Username);
        if (existingUser != null)
            throw new InvalidOperationException("Username already exists");

        var hashedPassword = passwordHasher.HashPassword(registerDto.Password);
        var user = new User {
            Id = Guid.NewGuid(),
            Username = registerDto.Username,
            Email = registerDto.Email,
            Name = registerDto.Name,
            AccessLevel = AccessLevel.Guest,
            PasswordHash = hashedPassword,
            CreatedDate = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _context.SaveChangesAsync();

        return UserReadDto.FromEntity(user);
    }

    public async Task<UserReadDto> UpdateUserAsync(Guid userId, UserUpdateDto updateDto) {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        if (!string.IsNullOrEmpty(updateDto.Username)) {
            var isUnique = await _userRepository.IsUsernameUniqueAsync(updateDto.Username, userId);
            if (!isUnique)
                throw new InvalidOperationException("Username already exists");
        }

        updateDto.ToEntity(user);
        await _context.SaveChangesAsync();

        return UserReadDto.FromEntity(user);
    }

    public async Task DeleteUserAsync(Guid userId) {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {userId} not found");

        await _userRepository.DeleteAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetUserByUsernameAsync(string username) {
        return await _userRepository.GetUserByUsernameAsync(username);
    }
}
