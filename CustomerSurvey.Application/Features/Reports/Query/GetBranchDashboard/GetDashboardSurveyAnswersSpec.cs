using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed class GetDashboardSurveyAnswersSpec
    : Specification<SurveyAnswer, DashboardSurveyAnswerDto>
{
    public GetDashboardSurveyAnswersSpec(
        Guid branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? templateId)
    {
        AddCriteria(x =>
            x.SurveyResponse.Template.BranchId == branchId &&
            x.SurveyResponse.SubmittedOnUtc >= fromUtc &&
            x.SurveyResponse.SubmittedOnUtc < toExclusiveUtc);

        if (templateId.HasValue)
        {
            AddCriteria(x => x.SurveyResponse.TemplateId == templateId.Value);
        }

        Select(x => new DashboardSurveyAnswerDto
        {
            SurveyResponseId = x.SurveyResponseId,
            TemplateId = x.SurveyResponse.TemplateId,
            TemplateNameEn = x.SurveyResponse.Template.NameEn,
            TemplateNameAr = x.SurveyResponse.Template.NameAr,
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