using ConferenceHub.Application.Interfaces;
using ConferenceHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHub.Infrastructure.Repositories;

public class Repository<T>(AppDbContext db) : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = db.Set<T>();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _set.FindAsync([id], ct).AsTask();

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _set.ToListAsync(ct);

    public IQueryable<T> Query() => _set.AsQueryable();

    public void Add(T entity) => _set.Add(entity);
    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
}
