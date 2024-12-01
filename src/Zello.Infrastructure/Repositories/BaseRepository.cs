using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Zello.Domain.RepositoryInterfaces;
using Zello.Infrastructure.Data;

namespace Zello.Infrastructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class {
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context) {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id) {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync() {
        return await _dbSet.ToListAsync();
    }

    public async Task<T> AddAsync(T entity) {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity) {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity) {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task SaveChangesAsync() {
        await _context.SaveChangesAsync();
    }
}
