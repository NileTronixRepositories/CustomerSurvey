using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed class GetDepartmentDashboardSurveyAnswersSpec
    : Specification<SurveyAnswer, DepartmentDashboardSurveyAnswerDto>
{
    public GetDepartmentDashboardSurveyAnswersSpec(
        Guid departmentId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? templateId)
    {
        AddCriteria(x =>
            x.SurveyResponse.Operator.DepartmentId == departmentId &&
            x.SurveyResponse.SubmittedOnUtc >= fromUtc &&
            x.SurveyResponse.SubmittedOnUtc < toExclusiveUtc);

        if (templateId.HasValue)
        {
            AddCriteria(x => x.SurveyResponse.TemplateId == templateId.Value);
        }

        Select(x => new DepartmentDashboardSurveyAnswerDto
        {
            SurveyResponseId = x.SurveyResponseId,
            TemplateId = x.SurveyResponse.TemplateId,
            TemplateNameEn = x.SurveyResponse.Template.NameEn,
            TemplateNameAr = x.SurveyResponse.Template.NameAr,
            OperatorId = x.SurveyResponse.OperatorId,
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
