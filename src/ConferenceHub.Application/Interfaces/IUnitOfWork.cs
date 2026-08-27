using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ConferenceHub.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken ct = default);
}
