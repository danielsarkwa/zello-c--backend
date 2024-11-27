using Microsoft.EntityFrameworkCore;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Interfaces;

public class UserRepository : IUserRepository {
    private readonly ApplicationDbContext _context;
    private readonly DbSet<User> _users;

    public UserRepository(ApplicationDbContext context) {
        _context = context;
        _users = context.Users;
    }

    public async Task<User?> GetByIdAsync(Guid id) {
        return await _users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllAsync() {
        return await _users.ToListAsync();
    }

    public async Task<User> AddAsync(User entity) {
        await _users.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(User entity) {
        _users.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User entity) {
        _users.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync() {
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByEmailAsync(string email) {
        return await _users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username) {
        return await _users.FirstOrDefaultAsync(u => u.Username == username);
    }
}
