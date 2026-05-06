using System.Linq.Expressions;

namespace BuildingBlock.Application.Abstraction.Persistence
{
    public interface IReadModelWriter<TEntity>
    where TEntity : class
    {
        Task AddAsync(TEntity entity, CancellationToken ct = default);

        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

        void Update(TEntity entity);

        void Remove(TEntity entity);

        Task<int> SaveChangesAsync(CancellationToken ct = default);

        // ✅ سريع للـ single primary key (مثلاً Guid Id)
        Task UpsertByPkAsync<TKey>(
            TEntity entity,
            TKey key,
            CancellationToken ct = default)
            where TKey : notnull;

        // ✅ عام للـ composite key أو أي match
        Task UpsertAsync(
            TEntity entity,
            Expression<Func<TEntity, bool>> match,
            Action<TEntity, TEntity>? map = null,
            CancellationToken ct = default);

        // ✅ update فقط لو موجود (من غير insert)
        Task<bool> TryUpdateAsync(
            Expression<Func<TEntity, bool>> match,
            Action<TEntity> update,
            CancellationToken ct = default);
    }
}