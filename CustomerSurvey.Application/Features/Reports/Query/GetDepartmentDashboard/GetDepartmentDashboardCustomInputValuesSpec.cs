using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetDepartmentDashboard;

internal sealed class GetDepartmentDashboardCustomInputValuesSpec
    : Specification<SurveyResponseCustomInputValue, DepartmentDashboardCustomInputValueDto>
{
    public GetDepartmentDashboardCustomInputValuesSpec(
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

        Select(x => new DepartmentDashboardCustomInputValueDto
        {
            SurveyResponseId = x.SurveyResponseId,
            NameSnapshot = x.NameSnapshot,
            TypeSnapshot = x.TypeSnapshot,
            StringValue = x.StringValue,
            IntegerValue = x.IntegerValue,
            ScorePercentage = x.SurveyResponse.ScorePercentage,
            MaxScore = x.SurveyResponse.MaxScore
        });
    }
}
