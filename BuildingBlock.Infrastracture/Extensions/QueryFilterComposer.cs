using System.Linq.Expressions;

namespace BuildingBlock.Infrastracture.Extensions
{
    internal static class QueryFilterComposer
    {
        public static LambdaExpression ComposeAnd(
            Type entityClrType,
            LambdaExpression? existingFilter,
            LambdaExpression newFilter)
        {
            if (existingFilter is null)
                return newFilter;

            var parameter = Expression.Parameter(entityClrType, "e");

            var left = ReplaceParameter(existingFilter, parameter);
            var right = ReplaceParameter(newFilter, parameter);

            var body = Expression.AndAlso(left.Body, right.Body);
            return Expression.Lambda(body, parameter);
        }

        private static LambdaExpression ReplaceParameter(LambdaExpression lambda, ParameterExpression newParam)
        {
            var oldParam = lambda.Parameters.Single();
            var visitor = new ReplaceParameterVisitor(oldParam, newParam);
            var newBody = visitor.Visit(lambda.Body)!;
            return Expression.Lambda(newBody, newParam);
        }

        private sealed class ReplaceParameterVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            public ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
            {
                _oldParam = oldParam;
                _newParam = newParam;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => node == _oldParam ? _newParam : base.VisitParameter(node);
        }
    }
}