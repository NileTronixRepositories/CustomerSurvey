using BuildingBlock.Application.Abstraction.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CustomerSurvey.infrastructure.Persistence
{
    internal sealed class EfReadModelWriter<TEntity> : IReadModelWriter<TEntity>
     where TEntity : class
    {
        private readonly PlatformReadDbContext _db;
        private readonly DbSet<TEntity> _set;

        public EfReadModelWriter(PlatformReadDbContext db)
        {
            _db = db;
            _set = _db.Set<TEntity>();
        }

        public Task AddAsync(TEntity entity, CancellationToken ct = default)
            => _set.AddAsync(entity, ct).AsTask();

        public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            _set.AddRange(entities);
            return Task.CompletedTask;
        }

        public void Update(TEntity entity) => _set.Update(entity);

        public void Remove(TEntity entity) => _set.Remove(entity);

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);

        public async Task UpsertByPkAsync<TKey>(TEntity entity, TKey key, CancellationToken ct = default)
            where TKey : notnull
        {
            var existing = await _set.FindAsync(new object[] { key }, ct);
            if (existing is null)
            {
                await _set.AddAsync(entity, ct);
                return;
            }

            _db.Entry(existing).CurrentValues.SetValues(entity);
        }

        public async Task UpsertAsync(
            TEntity entity,
            Expression<Func<TEntity, bool>> match,
            Action<TEntity, TEntity>? map = null,
            CancellationToken ct = default)
        {
            var existing = await _set.FirstOrDefaultAsync(match, ct);

            if (existing is null)
            {
                await _set.AddAsync(entity, ct);
                return;
            }

            if (map is null)
                _db.Entry(existing).CurrentValues.SetValues(entity);
            else
                map(entity, existing);
        }

        public async Task<bool> TryUpdateAsync(
            Expression<Func<TEntity, bool>> match,
            Action<TEntity> update,
            CancellationToken ct = default)
        {
            var existing = await _set.FirstOrDefaultAsync(match, ct);
            if (existing is null)
                return false;

            update(existing);
            return true;
        }
    }
}