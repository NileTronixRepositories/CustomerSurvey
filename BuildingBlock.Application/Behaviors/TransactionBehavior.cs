using BuildingBlock.Application.Abstraction.Persistence;
using MediatR;

namespace BuildingBlock.Application.Behaviors
{
    public interface ITransactionalRequest
    { } // علّم بيها الأوامر التي تحتاج معاملة

    public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IWriteDbContextAccessor? _accessor;

        public TransactionBehavior(IWriteDbContextAccessor? accessor = null)
            => _accessor = accessor;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            if (request is not ITransactionalRequest || _accessor is null)
                return await next();

            var db = _accessor.GetDbContext();

            await using var tx = await db.Database.BeginTransactionAsync(ct);
            try
            {
                var res = await next();
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return res;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }
}