using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Zello.Application.Dtos;
using Zello.Application.ServiceImplementations;
using Zello.Application.ServiceInterfaces;
using Zello.Domain.Entities;
using Zello.Domain.Entities.Api.User;
using Zello.Domain.RepositoryInterfaces;

namespace Zello.Tests.UnitTests.Services {
    public class UserServiceTests {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<DbContext> _mockDbContext;
        private readonly Mock<IPasswordHasher> _mockPasswordHasher;
        private readonly UserService _userService;

        public UserServiceTests() {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockDbContext = new Mock<DbContext>();
            _mockPasswordHasher = new Mock<IPasswordHasher>();
            _userService = new UserService(_mockUserRepository.Object, _mockDbContext.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_ExistingUser_ReturnsUserReadDto() {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                Name = "Test User",
                PasswordHash = "hashed_password"
            };

            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Username, result.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_NonExistingUser_ThrowsKeyNotFoundException() {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _userService.GetUserByIdAsync(userId)
            );
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers() {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "user1", Name = "User One", Email = "user1@example.com", PasswordHash = "hashed_password1" },
                new User { Id = Guid.NewGuid(), Username = "user2", Name = "User Two", Email = "user2@example.com", PasswordHash = "hashed_password2" }
            };

            _mockUserRepository
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.Equal(users.Count, result.Count());
            Assert.All(result, userDto =>
                Assert.Contains(users, u => u.Id == userDto.Id && u.Username == userDto.Username)
            );
        }

        [Fact]
        public async Task CreateUserAsync_UniqueUsername_CreatesUser() {
            // Arrange
            var createDto = new UserCreateDto {
                Username = "newuser",
                Email = "new@example.com",
                Name = "New User",
                Password = "password123",
                AccessLevel = AccessLevel.Member
            };

            _mockUserRepository
                .Setup(repo => repo.GetUserByUsernameAsync(createDto.Username))
                .ReturnsAsync((User)null);

            var hashedPassword = "hashed_password";
            _mockPasswordHasher
                .Setup(ph => ph.HashPassword(createDto.Password))
                .Returns(hashedPassword);

            _mockDbContext
                .Setup(ctx => ctx.SaveChangesAsync(default))
                .ReturnsAsync(1);

            // Act
            var result = await _userService.CreateUserAsync(createDto, _mockPasswordHasher.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createDto.Username, result.Username);
            Assert.Equal(createDto.Email, result.Email);

            _mockUserRepository.Verify(
                repo => repo.AddAsync(It.Is<User>(u =>
                    u.Username == createDto.Username &&
                    u.Email == createDto.Email &&
                    u.PasswordHash == hashedPassword
                )),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateUserAsync_DuplicateUsername_ThrowsInvalidOperationException() {
            // Arrange
            var createDto = new UserCreateDto {
                Username = "existinguser",
                Email = "existing@example.com",
                Password = "password123",
                Name = "Existing User",
                AccessLevel = AccessLevel.Member
            };

            var existingUser = new User {
                Username = createDto.Username,
                Name = createDto.Name,
                Email = createDto.Email,
                PasswordHash = "existing_password_hash"
            };
            _mockUserRepository
                .Setup(repo => repo.GetUserByUsernameAsync(createDto.Username))
                .ReturnsAsync(existingUser);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _userService.CreateUserAsync(createDto, _mockPasswordHasher.Object)
            );
        }

        [Fact]
        public async Task UpdateUserAsync_ExistingUser_UpdatesSuccessfully() {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User {
                Id = userId,
                Username = "oldusername",
                Email = "old@example.com",
                Name = "Old User",
                PasswordHash = "old_password_hash"
            };

            var updateDto = new UserUpdateDto {
                Username = "newusername",
                Email = "new@example.com"
            };

            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _mockUserRepository
                .Setup(repo => repo.IsUsernameUniqueAsync(updateDto.Username, userId))
                .ReturnsAsync(true);

            _mockDbContext
                .Setup(ctx => ctx.SaveChangesAsync(default))
                .ReturnsAsync(1);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Username, result.Username);
            Assert.Equal(updateDto.Email, result.Email);
        }

        [Fact]
        public async Task UpdateUserAsync_NonExistingUser_ThrowsKeyNotFoundException() {
            // Arrange
            var userId = Guid.NewGuid();
            var updateDto = new UserUpdateDto {
                Username = "newusername"
            };

            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _userService.UpdateUserAsync(userId, updateDto)
            );
        }

        [Fact]
        public async Task DeleteUserAsync_ExistingUser_DeletesSuccessfully() {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User {
                Id = userId,
                Username = "testuser",
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = "hashed_password"
            };

            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(existingUser);

            _mockDbContext
                .Setup(ctx => ctx.SaveChangesAsync(default))
                .ReturnsAsync(1);

            // Act
            await _userService.DeleteUserAsync(userId);

            // Assert
            _mockUserRepository.Verify(
                repo => repo.DeleteAsync(existingUser),
                Times.Once
            );
        }

        [Fact]
        public async Task DeleteUserAsync_NonExistingUser_ThrowsKeyNotFoundException() {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepository
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _userService.DeleteUserAsync(userId)
            );
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ExistingUser_ReturnsUser() {
            // Arrange
            var username = "testuser";
            var user = new User {
                Username = username,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = "hashed_password"
            };

            _mockUserRepository
                .Setup(repo => repo.GetUserByUsernameAsync(username))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetUserByUsernameAsync(username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
        }
    }
}