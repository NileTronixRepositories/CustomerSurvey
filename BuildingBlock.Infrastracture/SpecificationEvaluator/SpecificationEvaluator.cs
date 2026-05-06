using BuildingBlock.Domain.Enums;
using BuildingBlock.Domain.Specification;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlock.Infrastracture.SpecificationEvaluator
{
    public static class SpecificationEvaluator<TEntity> where TEntity : class
    {
        // ============== Data Query (full spec: includes/order/paging...) ==============
        public static IQueryable<TEntity> BuildQuery(
            IQueryable<TEntity> inputQuery,
            Specification<TEntity> spec)
            => BuildCore(inputQuery, spec, forCount: false);

        public static IQueryable<TOut> BuildQuery<TOut>(
            IQueryable<TEntity> inputQuery,
            Specification<TEntity, TOut> spec)
        {
            if (spec.Selector is null)
                throw new InvalidOperationException("Projection selector is not defined.");

            var entityQuery = BuildCore(inputQuery, spec, forCount: false);
            return entityQuery.Select(spec.Selector);
        }

        // ============== Count Query (filters only; no includes/order/paging) ==============
        public static IQueryable<TEntity> BuildCountQuery(
            IQueryable<TEntity> inputQuery,
            Specification<TEntity> spec)
            => BuildCore(inputQuery, spec, forCount: true);

        public static IQueryable<TEntity> BuildCountQuery<TOut>(
            IQueryable<TEntity> inputQuery,
            Specification<TEntity, TOut> spec)
            => BuildCore(inputQuery, spec, forCount: true);

        // ----------------------- Internal -----------------------
        private static IQueryable<TEntity> BuildCore(
            IQueryable<TEntity> inputQuery,
            Specification<TEntity> spec,
            bool forCount)
        {
            IQueryable<TEntity> query = inputQuery;

            // Ignore global filters
            if (spec.IsGlobalFiltersIgnored)
                query = query.IgnoreQueryFilters();

            // Criteria
            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria);

            // ✅ في حالة count: نرجّع هنا قبل paging/includes/order
            if (forCount)
                return query;

            // Tracking
            query = spec.Tracking switch
            {
                TrackingBehavior.NoTracking => query.AsNoTracking(),
                TrackingBehavior.NoTrackingWithIdentityResolution => query.AsNoTrackingWithIdentityResolution(),
                _ => query
            };

            // Includes
            if (spec.IncludePipelines.Count > 0)
            {
                foreach (var pipeline in spec.IncludePipelines)
                    query = pipeline(query);
            }

            // Ordering
            if (spec.OrderByDescendingExpressions.Count > 0)
            {
                IOrderedQueryable<TEntity>? ordered = null;
                foreach (var expr in spec.OrderByDescendingExpressions)
                    ordered = ordered is null ? query.OrderByDescending(expr) : ordered.ThenByDescending(expr);
                query = ordered!;
            }
            else if (spec.OrderByExpressions.Count > 0)
            {
                IOrderedQueryable<TEntity>? ordered = null;
                foreach (var expr in spec.OrderByExpressions)
                    ordered = ordered is null ? query.OrderBy(expr) : ordered.ThenBy(expr);
                query = ordered!;
            }

            // Distinct
            if (spec.IsDistinct)
                query = query.Distinct();

            // Paging
            if (spec.IsPagingEnabled)
                query = query.Skip(spec.Skip).Take(spec.Take);

            // Split / Single query toggle
            if (spec.IsSplitQuery)
                query = query.AsSplitQuery();
            else if (spec.IsSingleQuery)
                query = query.AsSingleQuery();

            // Transforms (TagWith, IgnoreAutoIncludes, Temporal, ...)
            if (spec.QueryTransforms.Count > 0)
            {
                foreach (var t in spec.QueryTransforms)
                    query = t(query);
            }

            return query;
        }
    }
}