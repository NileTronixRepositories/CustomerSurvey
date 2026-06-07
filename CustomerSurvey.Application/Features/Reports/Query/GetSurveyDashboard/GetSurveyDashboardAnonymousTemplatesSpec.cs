using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

internal sealed class GetSurveyDashboardAnonymousTemplatesSpec
    : Specification<AnonymousTemplate, SurveyDashboardTemplateRow>
{
    public GetSurveyDashboardAnonymousTemplatesSpec(
        Guid? branchId,
        Guid? anonymousTemplateId = null)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.Scope == AnonymousTemplateScope.Branch &&
            x.BranchId != null);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.BranchId == branchId.Value);
        }

        if (anonymousTemplateId.HasValue)
        {
            AddCriteria(x => x.Id == anonymousTemplateId.Value);
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

internal sealed class GetAnonymousTemplateForSurveyDashboardFilterSpec
    : Specification<AnonymousTemplate, ResolvedSurveyDashboardTemplateFilter>
{
    public GetAnonymousTemplateForSurveyDashboardFilterSpec(
        Guid anonymousTemplateId,
        Guid? branchId)
    {
        UseNoTracking();

        AddCriteria(x =>
            x.Id == anonymousTemplateId &&
            x.Scope == AnonymousTemplateScope.Branch &&
            x.BranchId != null);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.BranchId == branchId.Value);
        }

        Select(x => new ResolvedSurveyDashboardTemplateFilter
        {
            TemplateId = x.Id,
            TemplateKind = SurveyDashboardTemplateKind.Anonymous,
            DashboardSource = SurveyDashboardSource.Anonymous,
            BranchId = x.BranchId!.Value,
            NameEn = x.NameEn,
            NameAr = x.NameAr
        });
    }
}
