using BuildingBlock.Application.Repositories;
using BuildingBlock.Domain.Specification;
using BuildingBlock.Infrastracture.SpecificationEvaluator;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BuildingBlock.Infrastracture.Repositories
{
    public class EfGenericRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class
    {
        private readonly DbContext _context;
        private readonly DbSet<TEntity> _set;

        public EfGenericRepository(IDbContextProvider provider)
        {
            _context = provider.Context;
            _set = _context.Set<TEntity>();
        }

        // ===================== Basic CRUD =====================

        // ❗ خلي بالك: دي Sync وبتضرب DB. لو تقدر خليها Async.
        public bool HasData() => _set.Any();

        public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
            => _set.AddAsync(entity, cancellationToken).AsTask();

        public Task AddRangeAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
            => _set.AddRangeAsync(entities, cancellationToken);

        public void Delete(TEntity entity) => _set.Remove(entity);

        public void DeleteRange(IEnumerable<TEntity> entity) => _set.RemoveRange(entity);

        public void Update(TEntity entity) => _set.Update(entity);

        public void UpdateRange(IEnumerable<TEntity> entities) => _set.UpdateRange(entities);

        public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _set.FindAsync(new object?[] { id }, cancellationToken).AsTask();

        public Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => _set.FindAsync(new object?[] { id }, cancellationToken).AsTask();

        public Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => _set.FindAsync(new object?[] { id }, cancellationToken).AsTask();

        public Task<TEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => _set.FindAsync(new object?[] { id }, cancellationToken).AsTask();

        public Task<TEntity?> GetByPropertyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
            => _set.FirstOrDefaultAsync(predicate, cancellationToken);

        public Task<bool> IsExistAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            => _set.AnyAsync(filter, cancellationToken);

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken) > 0;

        public IReadOnlyList<TEntity> GetAll() => _set.AsNoTracking().ToList();

        public async Task<IEnumerable<TResult>> GetSelectedAsync<TResult>(
            Expression<Func<TEntity, TResult>> selector,
            Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = _set.AsQueryable();

            if (filter is not null)
                query = query.Where(filter);

            return await query.Select(selector).ToListAsync();
        }

        // ===================== Specification (Entity) =====================

        // ✅ بدل GetQuery القديمة: نبني Query للداتا + Query للـ count
        public IQueryable<TEntity> QueryWithSpec(Specification<TEntity> spec)
            => SpecificationEvaluator<TEntity>.BuildQuery(_set.AsQueryable(), spec);

        public IQueryable<TEntity> CountQueryWithSpec(Specification<TEntity> spec)
            => SpecificationEvaluator<TEntity>.BuildCountQuery(_set.AsQueryable(), spec);

        public async Task<List<TEntity>> ListWithSpecAsync(Specification<TEntity> spec, CancellationToken ct = default)
        {
            var query = QueryWithSpec(spec);
            return await query.ToListAsync(ct);
        }

        public async Task<TEntity?> FirstOrDefaultWithSpecAsync(Specification<TEntity> spec, CancellationToken ct = default)
        {
            var query = QueryWithSpec(spec);
            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<int> CountWithSpecAsync(Specification<TEntity> spec, CancellationToken ct = default)
        {
            if (!spec.IsTotalCountEnabled)
                return 0;

            var countQuery = CountQueryWithSpec(spec);
            return await countQuery.CountAsync(ct);
        }

        public async Task<bool> AnyWithSpecAsync(Specification<TEntity> spec, CancellationToken ct = default)
        {
            // Any لازم يتطبق على الفلاتر فقط (مش لازم paging/includes)
            var countQuery = CountQueryWithSpec(spec);
            return await countQuery.AnyAsync(ct);
        }

        public async Task<(List<TEntity> Data, int Count)> ListWithCountAsync(
            Specification<TEntity> spec,
            CancellationToken ct = default)
        {
            var count = await CountWithSpecAsync(spec, ct);
            var data = await ListWithSpecAsync(spec, ct);
            return (data, count);
        }

        // ===================== Specification (Projection) =====================

        public IQueryable<TOut> QueryWithSpec<TOut>(Specification<TEntity, TOut> spec)
            => SpecificationEvaluator<TEntity>.BuildQuery(_set.AsQueryable(), spec);

        public IQueryable<TEntity> CountQueryWithSpec<TOut>(Specification<TEntity, TOut> spec)
            => SpecificationEvaluator<TEntity>.BuildCountQuery(_set.AsQueryable(), spec);

        public async Task<List<TOut>> ListWithSpecAsync<TOut>(Specification<TEntity, TOut> spec, CancellationToken ct = default)
        {
            var query = QueryWithSpec(spec);
            return await query.ToListAsync(ct);
        }

        public async Task<TOut?> FirstOrDefaultWithSpecAsync<TOut>(Specification<TEntity, TOut> spec, CancellationToken ct = default)
        {
            var query = QueryWithSpec(spec);
            return await query.FirstOrDefaultAsync(ct);
        }

        public async Task<int> CountWithSpecAsync<TOut>(Specification<TEntity, TOut> spec, CancellationToken ct = default)
        {
            if (!spec.IsTotalCountEnabled)
                return 0;

            var countQuery = CountQueryWithSpec(spec);
            return await countQuery.CountAsync(ct);
        }

        public async Task<(List<TOut> Data, int Count)> ListWithCountAsync<TOut>(
            Specification<TEntity, TOut> spec,
            CancellationToken ct = default)
        {
            var count = await CountWithSpecAsync(spec, ct);
            var data = await ListWithSpecAsync(spec, ct);
            return (data, count);
        }

        // ===================== Optional: Upsert =====================

        public async Task UpsertAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate)
        {
            var existing = await _set.FirstOrDefaultAsync(predicate);

            if (existing is not null)
                _context.Entry(existing).CurrentValues.SetValues(entity);
            else
                await _set.AddAsync(entity);
        }
    }
}