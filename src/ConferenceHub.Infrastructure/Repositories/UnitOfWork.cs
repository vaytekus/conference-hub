using ConferenceHub.Application.Interfaces;
using ConferenceHub.Infrastructure.Data;

namespace ConferenceHub.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
