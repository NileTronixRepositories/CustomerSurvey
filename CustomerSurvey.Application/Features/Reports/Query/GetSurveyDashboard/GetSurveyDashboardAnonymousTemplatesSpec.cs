using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

internal sealed class GetSurveyDashboardAnonymousTemplatesSpec
    : Specification<AnonymousTemplate, SurveyDashboardTemplateRow>
{
    public GetSurveyDashboardAnonymousTemplatesSpec(Guid? branchId)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.Scope == AnonymousTemplateScope.Branch &&
            x.BranchId != null);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.BranchId == branchId.Value);
        }

        Select(x => new SurveyDashboardTemplateRow
        {
            Source = SurveyDashboardSource.Anonymous,
            TemplateId = x.Id,
            TemplateNameEn = x.NameEn,
            TemplateNameAr = x.NameAr,
            BranchId = x.BranchId!.Value,
            BranchNameEn = x.Branch!.NameEn,
            BranchNameAr = x.Branch.NameAr,
            IsActive = x.IsActive
        });
    }
}
