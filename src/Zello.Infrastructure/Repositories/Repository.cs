using Microsoft.EntityFrameworkCore;
using Zello.Infrastructure.Data;
using Zello.Infrastructure.Interfaces;

namespace Zello.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class {
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context) {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id) {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync() {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity) {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity) {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity) {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync() {
        await _context.SaveChangesAsync();
    }
}
