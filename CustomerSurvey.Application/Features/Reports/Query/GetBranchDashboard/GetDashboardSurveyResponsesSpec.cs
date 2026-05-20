using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed class GetDashboardSurveyResponsesSpec
    : Specification<SurveyResponse, DashboardSurveyResponseDto>
{
    public GetDashboardSurveyResponsesSpec(
        Guid branchId,
        DateTime fromUtc,
        DateTime toExclusiveUtc,
        Guid? templateId)
    {
        AddCriteria(x =>
            x.Template.BranchId == branchId &&
            x.SubmittedOnUtc >= fromUtc &&
            x.SubmittedOnUtc < toExclusiveUtc);

        if (templateId.HasValue)
        {
            AddCriteria(x => x.TemplateId == templateId.Value);
        }

        AddOrderByDescending(x => x.SubmittedOnUtc);

        Select(x => new DashboardSurveyResponseDto
        {
            SurveyResponseId = x.Id,
            TemplateId = x.TemplateId,
            TemplateNameEn = x.Template.NameEn,
            TemplateNameAr = x.Template.NameAr,
            SubmittedOnUtc = x.SubmittedOnUtc,
            ActualScore = x.ActualScore,
            MaxScore = x.MaxScore,
            ScorePercentage = x.ScorePercentage
        });
    }
}