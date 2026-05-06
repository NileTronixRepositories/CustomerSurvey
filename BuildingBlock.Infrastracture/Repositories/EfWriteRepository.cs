using BuildingBlock.Application.Abstraction.Persistence;
using BuildingBlock.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlock.Infrastracture.Repositories
{
    public class EfWriteRepository<TEntity, TWriteMarker> : IWriteRepository<TEntity, TWriteMarker>
     where TEntity : class
     where TWriteMarker : IWriteDbContextMarker
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _set;

        public EfWriteRepository(IDbContextResolver<TWriteMarker> resolver)
        {
            _context = resolver.Resolve();
            _set = _context.Set<TEntity>();
        }

        public Task AddAsync(TEntity entity, CancellationToken ct = default)
            => _set.AddAsync(entity, ct).AsTask();

        public Task AddRangeAsync(List<TEntity> entities, CancellationToken ct = default)
            => _set.AddRangeAsync(entities, ct);

        public void Update(TEntity entity) => _set.Update(entity);

        public void UpdateRange(IEnumerable<TEntity> entities) => _set.UpdateRange(entities);

        public void Delete(TEntity entity) => _set.Remove(entity);

        public void DeleteRange(IEnumerable<TEntity> entities) => _set.RemoveRange(entities);
    }
}