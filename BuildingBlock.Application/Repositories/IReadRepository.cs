using BuildingBlock.Application.Abstraction.Persistence;
using BuildingBlock.Domain.Specification;
using System.Linq.Expressions;

namespace BuildingBlock.Application.Repositories
{
    public interface IReadRepository<TEntity, TReadMarker>
     where TEntity : class
     where TReadMarker : IReadDbContextMarker
    {
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<TEntity?> GetByIdAsync(long id, CancellationToken ct = default);

        Task<TEntity?> GetByIdAsync(string id, CancellationToken ct = default);

        Task<TEntity?> GetByIdTrackedAsync(Guid id, CancellationToken ct = default);

        Task<TEntity?> GetByIdTrackedAsync(int id, CancellationToken ct = default);

        Task<TEntity?> GetByIdTrackedAsync(long id, CancellationToken ct = default);

        Task<TEntity?> GetByIdTrackedAsync(string id, CancellationToken ct = default);

        Task<TEntity?> GetByPropertyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

        Task<TEntity?> GetByPropertyTrackedAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);

        Task<List<TEntity>> ListAsync(Specification<TEntity> spec, CancellationToken ct = default);

        Task<TEntity?> FirstOrDefaultAsync(Specification<TEntity> spec, CancellationToken ct = default);

        Task<List<TOut>> ListAsync<TOut>(Specification<TEntity, TOut> spec, CancellationToken ct = default);

        Task<TOut?> FirstOrDefaultAsync<TOut>(Specification<TEntity, TOut> spec, CancellationToken ct = default);

        IQueryable<TEntity> Query();

        Task<(List<TEntity> Data, int Count)> ListWithCountAsync(
           Specification<TEntity> spec,
           CancellationToken ct = default);

        Task<(List<TOut> Data, int Count)> ListWithCountAsync<TOut>(
            Specification<TEntity, TOut> spec,
            CancellationToken ct = default);
    }
}