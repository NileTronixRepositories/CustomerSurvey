using BuildingBlock.Application.Abstraction.Persistence;

namespace BuildingBlock.Application.Repositories
{
    public interface IWriteRepository<TEntity, TWriteMarker>
     where TEntity : class
     where TWriteMarker : IWriteDbContextMarker
    {
        Task AddAsync(TEntity entity, CancellationToken ct = default);

        Task AddRangeAsync(List<TEntity> entities, CancellationToken ct = default);

        void Update(TEntity entity);

        void UpdateRange(IEnumerable<TEntity> entities);

        void Delete(TEntity entity);

        void DeleteRange(IEnumerable<TEntity> entities);
    }
}