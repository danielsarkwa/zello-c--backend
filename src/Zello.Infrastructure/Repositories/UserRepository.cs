using Zello.Application.Interfaces;
using Zello.Domain.Entities.Dto;
using Zello.Infrastructure.TestingDataStorage;

namespace Zello.Infrastructure.Repositories;

public class UserRepository : IUserRepository {
    public UserDto? FindByUsername(string username) {
        return TestData.FindUserByUsername(username);
    }
}
