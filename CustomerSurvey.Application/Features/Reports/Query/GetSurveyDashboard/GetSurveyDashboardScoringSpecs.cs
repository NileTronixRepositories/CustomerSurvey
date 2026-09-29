using BuildingBlock.Domain.Specification;
using CustomerSurvey.Application.Features.Reports.Query.GetBranchTemplatesPdfReport;
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
            TemplateKind = ReportTemplateKind.Normal,
            TemplateQuestionId = x.Id,
            TemplateId = x.TemplateId,
            QuestionId = x.QuestionId,
            Order = x.Order,
            QuestionTextEn = x.Question.TextEn,
            QuestionTextAr = x.Question.TextAr,
            QuestionType = x.Question.Type,
            QuestionGroupId = x.Question.GroupId,
            QuestionGroupNameEn = x.Question.Group.NameEn,
            QuestionGroupNameAr = x.Question.Group.NameAr
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
            TemplateKind = ReportTemplateKind.Anonymous,
            TemplateQuestionId = x.Id,
            TemplateId = x.AnonymousTemplateId,
            QuestionId = x.QuestionId,
            Order = x.Order,
            QuestionTextEn = x.Question.TextEn,
            QuestionTextAr = x.Question.TextAr,
            QuestionType = x.Question.Type,
            QuestionGroupId = x.Question.GroupId,
            QuestionGroupNameEn = x.Question.Group.NameEn,
            QuestionGroupNameAr = x.Question.Group.NameAr
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
            TemplateKind = ReportTemplateKind.Normal,
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
            TemplateKind = ReportTemplateKind.Anonymous,
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
