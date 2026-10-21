using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
using CustomerSurvey.Domain.Entities;
using System.Linq.Expressions;

namespace CustomerSurvey.Application.Features.Reports.Shared;

// Keeps score filtering, ordering, count and paging in SQL, using immutable submission-derived columns.
internal abstract class ScoredResponseSpecification<TEntity, TOut>(ScoreCalculationMode mode) : Specification<TEntity, TOut>
    where TEntity : class
{
    protected new void AddCriteria(Expression<Func<TEntity, bool>> predicate) => base.AddCriteria(Rewrite(predicate));
    protected new void Select(Expression<Func<TEntity, TOut>> selector) => base.Select(Rewrite(selector));
    protected new void AddOrderBy(Expression<Func<TEntity, object>> key) => base.AddOrderBy(Rewrite(key));
    protected new void AddOrderByDescending(Expression<Func<TEntity, object>> key) => base.AddOrderByDescending(Rewrite(key));

    protected void ApplyIncludedQuestionFilter(Guid? questionId, bool includedOnly)
    {
        if (!includedOnly || !questionId.HasValue) return;
        var parameter = Expression.Parameter(typeof(TEntity), "response");
        var snapshot = Expression.Property(parameter, "Snapshot");
        var captured = Expression.AndAlso(Expression.NotEqual(snapshot, Expression.Constant(null, typeof(ResponseSnapshot))),
            Expression.Equal(Expression.Property(snapshot, nameof(ResponseSnapshot.Provenance)), Expression.Constant(SnapshotProvenance.CapturedAtSubmission)));
        var prefix = mode == ScoreCalculationMode.RootQuestions ? "Root" : mode == ScoreCalculationMode.LowestConditionLevel ? "Lowest" : "All";
        var contains = Expression.Call(Expression.Property(snapshot, prefix + "QuestionIds"),
            typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!, Expression.Constant("|" + questionId.Value + "|"));
        AddCriteria(Expression.Lambda<Func<TEntity, bool>>(Expression.AndAlso(captured, contains), parameter));
    }

    private Expression<T> Rewrite<T>(Expression<T> expression) => (Expression<T>)new ScoreVisitor(mode).Visit(expression)!;

    private sealed class ScoreVisitor(ScoreCalculationMode mode) : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression?.Type == typeof(TEntity) && node.Member.Name is "MaxScore" or "ScorePercentage")
            {
                var snapshot = Expression.Property(node.Expression, "Snapshot");
                var captured = Expression.AndAlso(Expression.NotEqual(snapshot, Expression.Constant(null, typeof(ResponseSnapshot))),
                    Expression.Equal(Expression.Property(snapshot, nameof(ResponseSnapshot.Provenance)),
                        Expression.Constant(SnapshotProvenance.CapturedAtSubmission)));
                var prefix = mode == ScoreCalculationMode.RootQuestions ? "Root" : mode == ScoreCalculationMode.LowestConditionLevel ? "Lowest" : "All";
                Expression selected = node.Member.Name == "MaxScore"
                    ? Expression.Multiply(Expression.Property(snapshot, prefix + "ScoredItemsCount"), Expression.Constant(5))
                    : Expression.Coalesce(Expression.Property(snapshot, prefix + "ScorePercentage"), Expression.Constant(0m));
                return Expression.Condition(captured, selected, node);
            }
            return base.VisitMember(node);
        }
    }
}
