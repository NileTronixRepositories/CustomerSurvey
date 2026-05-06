using BuildingBlock.Application.Abstraction.Persistence;
using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Specification;
using BuildingBlock.Infrastracture.SpecificationEvaluator;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BuildingBlock.Infrastracture.Repositories
{
    public class EfReadRepository<TEntity, TReadMarker> : IReadRepository<TEntity, TReadMarker>
       where TEntity : class
       where TReadMarker : IReadDbContextMarker
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _set;

        public EfReadRepository(IDbContextResolver<TReadMarker> resolver)
        {
            _context = resolver.Resolve();
            _set = _context.Set<TEntity>();
        }

        public IQueryable<TEntity> Query() => _set.AsNoTracking();

        public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => _set.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);

        public Task<TEntity?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default)
            => _set.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, ct);

        public Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
            => _set.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);

        public Task<TEntity?> GetByIdTrackedAsync(int id, CancellationToken ct = default)
            => _set.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id, ct);

        public Task<TEntity?> GetByIdAsync(long id, CancellationToken ct = default)
            => _set.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<long>(e, "Id") == id, ct);

        public Task<TEntity?> GetByIdTrackedAsync(long id, CancellationToken ct = default)
            => _set.FirstOrDefaultAsync(e => EF.Property<long>(e, "Id") == id, ct);

        public Task<TEntity?> GetByIdAsync(string id, CancellationToken ct = default)
            => _set.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id, ct);

        public Task<TEntity?> GetByIdTrackedAsync(string id, CancellationToken ct = default)
            => _set.FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id, ct);

        public Task<TEntity?> GetByPropertyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            => _set.AsNoTracking().FirstOrDefaultAsync(predicate, ct);

        public Task<TEntity?> GetByPropertyTrackedAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            => _set.FirstOrDefaultAsync(predicate, ct);

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
            => _set.AsNoTracking().AnyAsync(predicate, ct);

        public Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
            => predicate is null
                ? _set.AsNoTracking().CountAsync(ct)
                : _set.AsNoTracking().CountAsync(predicate, ct);

        public async Task<List<TEntity>> ListAsync(Specification<TEntity> spec, CancellationToken ct = default)
        {
            var query = SpecificationEvaluator<TEntity>.BuildQuery(_set.AsQueryable(), spec);
            return await query.ToListAsync(ct);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Specification<TEntity> spec, CancellationToken ct = default)
        {
            var query = SpecificationEvaluator<TEntity>.BuildQuery(_set.AsQueryable(), spec);
            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<List<TOut>> ListAsync<TOut>(Specification<TEntity, TOut> spec, CancellationToken ct = default)
        {
            var query = SpecificationEvaluator<TEntity>.BuildQuery(_set.AsQueryable(), spec);
            return await query.ToListAsync(ct);
        }

        public async Task<TOut?> FirstOrDefaultAsync<TOut>(Specification<TEntity, TOut> spec, CancellationToken ct = default)
        {
            var query = SpecificationEvaluator<TEntity>.BuildQuery(_set.AsQueryable(), spec);
            return await query.FirstOrDefaultAsync(ct);
        }

        // ===================== NEW: ListWithCountAsync =====================

        public async Task<(List<TEntity> Data, int Count)> ListWithCountAsync(
            Specification<TEntity> spec,
            CancellationToken ct = default)
        {
            var baseQuery = _set.AsQueryable();

            var count = 0;
            if (spec.IsTotalCountEnabled)
            {
                var countQuery = SpecificationEvaluator<TEntity>.BuildCountQuery(baseQuery, spec);
                count = await countQuery.CountAsync(ct);
            }

            var dataQuery = SpecificationEvaluator<TEntity>.BuildQuery(baseQuery, spec);
            var data = await dataQuery.ToListAsync(ct);

            return (data, count);
        }

        public async Task<(List<TOut> Data, int Count)> ListWithCountAsync<TOut>(
            Specification<TEntity, TOut> spec,
            CancellationToken ct = default)
        {
            var baseQuery = _set.AsQueryable();

            var count = 0;
            if (spec.IsTotalCountEnabled)
            {
                var countQuery = SpecificationEvaluator<TEntity>.BuildCountQuery(baseQuery, spec);
                count = await countQuery.CountAsync(ct);
            }

            var dataQuery = SpecificationEvaluator<TEntity>.BuildQuery(baseQuery, spec);
            var data = await dataQuery.ToListAsync(ct);

            return (data, count);
        }
    }
}