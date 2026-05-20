using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed class GetSystemDashboardSurveyAnswersSpec
    : Specification<SurveyAnswer, SystemDashboardSurveyAnswerDto>
{
    public GetSystemDashboardSurveyAnswersSpec(
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? branchId,
        Guid? departmentId)
    {
        AddCriteria(x =>
            x.SurveyResponse.SubmittedOnUtc >= fromUtc &&
            x.SurveyResponse.SubmittedOnUtc < toExclusiveUtc);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.SurveyResponse.Template.BranchId == branchId.Value);
        }

        if (departmentId.HasValue)
        {
            AddCriteria(x => x.SurveyResponse.Operator.DepartmentId == departmentId.Value);
        }

        Select(x => new SystemDashboardSurveyAnswerDto
        {
            SurveyResponseId = x.SurveyResponseId,
            TemplateId = x.SurveyResponse.TemplateId,
            BranchId = x.SurveyResponse.Template.BranchId,
            DepartmentId = x.SurveyResponse.Operator.DepartmentId,
            QuestionType = x.QuestionType,
            TextAnswer = x.TextAnswer
        });
    }
}