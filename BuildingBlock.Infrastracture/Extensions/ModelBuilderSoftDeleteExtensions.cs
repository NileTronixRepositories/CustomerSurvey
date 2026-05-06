using BuildingBlock.Domain.Primitive;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BuildingBlock.Infrastracture.Extensions
{
    public static class ModelBuilderSoftDeleteExtensions
    {
        /// <summary>
        /// يطبّق HasQueryFilter(e => !e.IsDeleted) لكل EntityType ينفّذ ISoftDeleteEntity.
        /// استدعِها داخل OnModelCreating بعد base.OnModelCreating.
        /// </summary>
        public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(ISoftDeleteEntity).IsAssignableFrom(entityType.ClrType))
                    continue;

                var clrType = entityType.ClrType;

                var parameter = Expression.Parameter(clrType, "e");
                var prop = Expression.Property(parameter, nameof(ISoftDeleteEntity.IsDeleted));
                var body = Expression.Equal(prop, Expression.Constant(false));
                var newFilter = Expression.Lambda(body, parameter);

                var existing = entityType.GetQueryFilter();
                var merged = QueryFilterComposer.ComposeAnd(clrType, existing, newFilter);

                entityType.SetQueryFilter(merged);
            }
        }
    }
}

/*
 protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.ApplySoftDeleteQueryFilter();
}
*/