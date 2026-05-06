using BuildingBlock.Application.Abstraction.Persistence;
using BuildingBlock.Application.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;
using System.Data;

namespace BuildingBlock.Infrastracture.Repositories
{
    public class EfUnitOfWork<TWriteMarker> : IUnitOfWork<TWriteMarker>
    where TWriteMarker : IWriteDbContextMarker
    {
        private readonly DbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repos = new();

        public EfUnitOfWork(IDbContextResolver<TWriteMarker> resolver)
            => _context = resolver.Resolve();

        public IWriteRepository<TEntity, TWriteMarker> WriteRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            if (_repos.TryGetValue(type, out var repo))
                return (IWriteRepository<TEntity, TWriteMarker>)repo;

            // resolve by DI each time is ok, but we keep same context by inline resolver
            var instance = new EfWriteRepository<TEntity, TWriteMarker>(new InlineResolver<TWriteMarker>(_context));
            _repos[type] = instance;
            return instance;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
            => _context.Database.BeginTransactionAsync(ct);

        public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default)
            => _context.Database.BeginTransactionAsync(isolationLevel, ct);

        public Task CommitTransactionAsync(CancellationToken ct = default)
            => _context.Database.CommitTransactionAsync(ct);

        public Task RollbackTransactionAsync(CancellationToken ct = default)
            => _context.Database.RollbackTransactionAsync(ct);

        private sealed class InlineResolver<TM> : IDbContextResolver<TM>
        {
            private readonly DbContext _ctx;

            public InlineResolver(DbContext ctx) => _ctx = ctx;

            public DbContext Resolve() => _ctx;
        }
    }
}