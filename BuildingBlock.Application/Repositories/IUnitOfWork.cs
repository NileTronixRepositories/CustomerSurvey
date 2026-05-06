using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace BuildingBlock.Application.Repositories
{
    public interface IUnitOfWork<TWriteMarker>
     where TWriteMarker : IWriteDbContextMarker
    {
        IWriteRepository<TEntity, TWriteMarker> WriteRepository<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default);

        Task CommitTransactionAsync(CancellationToken ct = default);

        Task RollbackTransactionAsync(CancellationToken ct = default);
    }
}