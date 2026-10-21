using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;
using System.Linq.Expressions;

namespace CustomerSurvey.Application.Features.Reports.Shared;

// Reuse the existing authorized response criteria and projection, while paging complaint answers.
internal sealed class ComplaintItemsSpecification<TOut> : Specification<SurveyAnswer, TOut>
{
    public ComplaintItemsSpecification(Specification<SurveyResponse, TOut> responses, int page, int size, Guid? questionId)
    {
        var answer = Expression.Parameter(typeof(SurveyAnswer), "answer");
        var response = Expression.Property(answer, nameof(SurveyAnswer.SurveyResponse));
        var criteria = new Substitute(responses.Criteria.Parameters[0], response).Visit(responses.Criteria.Body)!;
        AddCriteria(Expression.Lambda<Func<SurveyAnswer, bool>>(criteria, answer));
        AddCriteria(x => x.QuestionType == QuestionType.Complain && x.TextAnswer != null && x.TextAnswer.Trim() != "");
        if (questionId.HasValue) AddCriteria(x => x.QuestionId == questionId);
        var original = (MemberInitExpression)new Substitute(responses.Selector.Parameters[0], response).Visit(responses.Selector.Body)!;
        var bindings = original.Bindings.Concat(new[]
        {
            Expression.Bind(typeof(TOut).GetProperty("ComplaintQuestionId")!, Expression.Convert(Expression.Property(answer, "QuestionId"), typeof(Guid?))),
            Expression.Bind(typeof(TOut).GetProperty("ComplaintText")!, Expression.Property(answer, "TextAnswer"))
        });
        Select(Expression.Lambda<Func<SurveyAnswer, TOut>>(Expression.MemberInit(original.NewExpression, bindings), answer));
        AddOrderByDescending(x => x.SurveyResponse.SubmittedOnUtc);
        AddOrderBy(x => x.Id);
        UseNoTracking();
        EnableTotalCount();
        ApplyPaging(page, size);
    }

    private sealed class Substitute(ParameterExpression source, Expression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == source ? target : base.VisitParameter(node);
    }
}

internal sealed class AnonymousComplaintItemsSpecification<TOut> : Specification<AnonymousSurveyAnswer, TOut>
{
    public AnonymousComplaintItemsSpecification(Specification<AnonymousSurveyResponse, TOut> responses, int page, int size, Guid? questionId)
    {
        var answer = Expression.Parameter(typeof(AnonymousSurveyAnswer), "answer");
        var response = Expression.Property(answer, nameof(AnonymousSurveyAnswer.AnonymousSurveyResponse));
        var criteria = new Substitute(responses.Criteria.Parameters[0], response).Visit(responses.Criteria.Body)!;
        AddCriteria(Expression.Lambda<Func<AnonymousSurveyAnswer, bool>>(criteria, answer));
        AddCriteria(x => x.QuestionType == QuestionType.Complain && x.TextAnswer != null && x.TextAnswer.Trim() != "");
        if (questionId.HasValue) AddCriteria(x => x.QuestionId == questionId);
        var original = (MemberInitExpression)new Substitute(responses.Selector.Parameters[0], response).Visit(responses.Selector.Body)!;
        var bindings = original.Bindings.Concat(new[]
        {
            Expression.Bind(typeof(TOut).GetProperty("ComplaintQuestionId")!, Expression.Convert(Expression.Property(answer, "QuestionId"), typeof(Guid?))),
            Expression.Bind(typeof(TOut).GetProperty("ComplaintText")!, Expression.Property(answer, "TextAnswer"))
        });
        Select(Expression.Lambda<Func<AnonymousSurveyAnswer, TOut>>(Expression.MemberInit(original.NewExpression, bindings), answer));
        AddOrderByDescending(x => x.AnonymousSurveyResponse.SubmittedOnUtc);
        AddOrderBy(x => x.Id);
        UseNoTracking();
        EnableTotalCount();
        ApplyPaging(page, size);
    }

    private sealed class Substitute(ParameterExpression source, Expression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => node == source ? target : base.VisitParameter(node);
    }
}
