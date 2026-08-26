namespace ConferenceHub.Application.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    IQueryable<T> Query();

    void Add(T entity);
    void Update(T entity);
    void Remove(T entity);
}
