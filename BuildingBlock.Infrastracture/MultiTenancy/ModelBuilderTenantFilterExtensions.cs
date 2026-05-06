using BuildingBlock.Domain.Primitive;
using BuildingBlock.Infrastracture.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace BuildingBlock.Infrastracture.MultiTenancy
{
    public static class ModelBuilderTenantFilterExtensions
    {
        public static void ApplyTenantQueryFilters(this ModelBuilder modelBuilder, DbContext dbContext)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                    continue;

                var clrType = entityType.ClrType;

                var newFilter = BuildTenantFilterExpression(entityType, dbContext);

                var existing = entityType.GetQueryFilter();
                var merged = QueryFilterComposer.ComposeAnd(clrType, existing, newFilter);

                entityType.SetQueryFilter(merged);
            }
        }

        private static LambdaExpression BuildTenantFilterExpression(IMutableEntityType entityType, DbContext dbContext)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");

            // e.AccountId (Guid)
            var entityAccountId = Expression.Property(parameter, nameof(ITenantEntity.AccountId));

            var dbContextConst = Expression.Constant(dbContext);
            var typedDbContext = Expression.Convert(dbContextConst, dbContext.GetType());

            var currentAccountIdProp = Expression.Property(typedDbContext, "CurrentAccountId"); // Guid?
            var isPlatformAdminProp = Expression.Property(typedDbContext, "IsPlatformAdmin");   // bool

            // (Guid?)e.AccountId == CurrentAccountId
            var entityAccountIdNullable = Expression.Convert(entityAccountId, typeof(Guid?));
            var equals = Expression.Equal(entityAccountIdNullable, currentAccountIdProp);

            // IsPlatformAdmin || equals
            var body = Expression.OrElse(isPlatformAdminProp, equals);

            return Expression.Lambda(body, parameter);
        }
    }
}