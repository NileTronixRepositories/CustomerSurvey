using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.Reports.Services.Scoring;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

internal sealed class GetSurveyDashboardTemplateQuestionsSpec
    : Specification<TemplateQuestion, TemplateQuestionFlatDto>
{
    public GetSurveyDashboardTemplateQuestionsSpec(IReadOnlyCollection<Guid> templateIds)
    {
        UseNoTracking();

        AddCriteria(x => templateIds.Contains(x.TemplateId));

        Select(x => new TemplateQuestionFlatDto
        {
            TemplateQuestionId = x.Id,
            TemplateId = x.TemplateId,
            QuestionId = x.QuestionId,
            Order = x.Order,
            QuestionTextEn = x.Question.TextEn,
            QuestionTextAr = x.Question.TextAr,
            QuestionType = x.Question.Type
        });
    }
}

internal sealed class GetSurveyDashboardAnonymousTemplateQuestionsSpec
    : Specification<AnonymousTemplateQuestion, TemplateQuestionFlatDto>
{
    public GetSurveyDashboardAnonymousTemplateQuestionsSpec(IReadOnlyCollection<Guid> templateIds)
    {
        UseNoTracking();

        AddCriteria(x => templateIds.Contains(x.AnonymousTemplateId));

        Select(x => new TemplateQuestionFlatDto
        {
            TemplateQuestionId = x.Id,
            TemplateId = x.AnonymousTemplateId,
            QuestionId = x.QuestionId,
            Order = x.Order,
            QuestionTextEn = x.Question.TextEn,
            QuestionTextAr = x.Question.TextAr,
            QuestionType = x.Question.Type
        });
    }
}

internal sealed class GetSurveyDashboardTemplateQuestionConditionsSpec
    : Specification<TemplateQuestionCondition, ConditionFlatDto>
{
    public GetSurveyDashboardTemplateQuestionConditionsSpec(IReadOnlyCollection<Guid> templateIds)
    {
        UseNoTracking();

        AddCriteria(x =>
            templateIds.Contains(x.TemplateId) &&
            x.IsActive);

        Select(x => new ConditionFlatDto
        {
            TemplateId = x.TemplateId,
            ParentTemplateQuestionId = x.ParentTemplateQuestionId,
            ChildTemplateQuestionId = x.ChildTemplateQuestionId,
            TriggerType = x.TriggerType,
            SelectedQuestionOptionId = x.SelectedQuestionOptionId,
            TriggerValue = x.TriggerValue
        });
    }
}

internal sealed class GetSurveyDashboardAnonymousTemplateQuestionConditionsSpec
    : Specification<AnonymousTemplateQuestionCondition, ConditionFlatDto>
{
    public GetSurveyDashboardAnonymousTemplateQuestionConditionsSpec(IReadOnlyCollection<Guid> templateIds)
    {
        UseNoTracking();

        AddCriteria(x =>
            templateIds.Contains(x.AnonymousTemplateId) &&
            x.IsActive);

        Select(x => new ConditionFlatDto
        {
            TemplateId = x.AnonymousTemplateId,
            ParentTemplateQuestionId = x.ParentAnonymousTemplateQuestionId,
            ChildTemplateQuestionId = x.ChildAnonymousTemplateQuestionId,
            TriggerType = x.TriggerType,
            SelectedQuestionOptionId = x.SelectedQuestionOptionId,
            TriggerValue = x.TriggerValue
        });
    }
}

internal sealed class GetSurveyDashboardQuestionOptionsSpec
    : Specification<QuestionOption, QuestionOptionFlatDto>
{
    public GetSurveyDashboardQuestionOptionsSpec(IReadOnlyCollection<Guid> questionIds)
    {
        UseNoTracking();

        AddCriteria(x =>
            questionIds.Contains(x.QuestionId) &&
            x.IsActive);

        Select(x => new QuestionOptionFlatDto
        {
            OptionId = x.Id,
            QuestionId = x.QuestionId,
            TextEn = x.TextEn,
            TextAr = x.TextAr,
            Value = x.Value,
            Order = x.Order
        });
    }
}
