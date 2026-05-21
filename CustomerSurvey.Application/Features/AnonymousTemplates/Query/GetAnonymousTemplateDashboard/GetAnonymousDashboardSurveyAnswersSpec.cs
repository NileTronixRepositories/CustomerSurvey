using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed class GetAnonymousDashboardSurveyAnswersSpec
    : Specification<AnonymousSurveyAnswer, AnonymousDashboardSurveyAnswerDto>
{
    public GetAnonymousDashboardSurveyAnswersSpec(
        Guid branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? anonymousTemplateId)
    {
        AddCriteria(x =>
            x.AnonymousSurveyResponse.AnonymousTemplate.Scope == AnonymousTemplateScope.Branch &&
            x.AnonymousSurveyResponse.AnonymousTemplate.BranchId == branchId &&
            x.AnonymousSurveyResponse.SubmittedOnUtc >= fromUtc &&
            x.AnonymousSurveyResponse.SubmittedOnUtc < toExclusiveUtc);

        if (anonymousTemplateId.HasValue)
        {
            AddCriteria(x => x.AnonymousSurveyResponse.AnonymousTemplateId == anonymousTemplateId.Value);
        }

        Select(x => new AnonymousDashboardSurveyAnswerDto
        {
            AnonymousSurveyResponseId = x.AnonymousSurveyResponseId,
            AnonymousTemplateId = x.AnonymousSurveyResponse.AnonymousTemplateId,
            TemplateNameEn = x.AnonymousSurveyResponse.AnonymousTemplate.NameEn,
            TemplateNameAr = x.AnonymousSurveyResponse.AnonymousTemplate.NameAr,
            AnonymousTemplateQuestionId = x.AnonymousTemplateQuestionId,
            QuestionId = x.QuestionId,
            QuestionTextEn = x.Question.TextEn,
            QuestionTextAr = x.Question.TextAr,
            QuestionType = x.QuestionType,
            StarRatingValue = x.StarRatingValue,
            SmileValue = x.SmileValue,
            TextAnswer = x.TextAnswer,
            VoiceFileName = x.VoiceFileName
        });
    }
}
