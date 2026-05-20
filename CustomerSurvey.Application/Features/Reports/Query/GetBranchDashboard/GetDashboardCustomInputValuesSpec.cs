using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed class GetDashboardCustomInputValuesSpec
    : Specification<SurveyResponseCustomInputValue, DashboardCustomInputValueDto>
{
    public GetDashboardCustomInputValuesSpec(
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

        Select(x => new DashboardCustomInputValueDto
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