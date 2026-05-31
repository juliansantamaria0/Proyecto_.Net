using System.Linq.Expressions;
using AutoTallerManager.Domain.Common;
using AutoTallerManager.Domain.Ports.Output;
using AutoTallerManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoTallerManager.Infrastructure.Repositories;

public class GenericRepository<T>(AutoTallerDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return await query.ToListAsync();
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet.Where(predicate);
        query = includes.Aggregate(query, (current, include) => current.Include(include));
        return await query.ToListAsync();
    }

    public async Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool descending = false,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = DbSet;

        if (filter != null)
            query = query.Where(filter);

        query = includes.Aggregate(query, (current, include) => current.Include(include));

        var totalCount = await query.CountAsync();

        if (orderBy != null)
            query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
        else
            query = query.OrderBy(e => e.Id);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(T entity) => await DbSet.AddAsync(entity);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
        await DbSet.AnyAsync(predicate);
}
