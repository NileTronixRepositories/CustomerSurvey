using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSurveyDashboard;

internal sealed class GetSurveyDashboardTemplatesSpec
    : Specification<Template, SurveyDashboardTemplateRow>
{
    public GetSurveyDashboardTemplatesSpec(
        Guid? branchId,
        Guid? templateId = null)
    {
        UseNoTracking();

        if (branchId.HasValue)
        {
            AddCriteria(x => x.BranchId == branchId.Value);
        }

        if (templateId.HasValue)
        {
            AddCriteria(x => x.Id == templateId.Value);
        }

        Select(x => new SurveyDashboardTemplateRow
        {
            Source = SurveyDashboardSource.Internal,
            TemplateId = x.Id,
            TemplateNameEn = x.NameEn,
            TemplateNameAr = x.NameAr,
            BranchId = x.BranchId,
            BranchNameEn = x.Branch.NameEn,
            BranchNameAr = x.Branch.NameAr,
            IsActive = x.IsActive
        });
    }
}

internal sealed class GetAuthorizedTemplateForSurveyDashboardFilterSpec
    : Specification<Template, ResolvedSurveyDashboardTemplateFilter>
{
    public GetAuthorizedTemplateForSurveyDashboardFilterSpec(
        Guid templateId,
        Guid? branchId)
    {
        UseNoTracking();

        AddCriteria(x => x.Id == templateId);

        if (branchId.HasValue)
        {
            AddCriteria(x => x.BranchId == branchId.Value);
        }

        Select(x => new ResolvedSurveyDashboardTemplateFilter
        {
            TemplateId = x.Id,
            TemplateKind = SurveyDashboardTemplateKind.Authorized,
            DashboardSource = SurveyDashboardSource.Internal,
            BranchId = x.BranchId,
            NameEn = x.NameEn,
            NameAr = x.NameAr
        });
    }
}

internal sealed class GetSurveyDashboardBranchesSpec
    : Specification<Branch, SurveyDashboardBranchRow>
{
    public GetSurveyDashboardBranchesSpec(Guid? branchId)
    {
        UseNoTracking();

        if (branchId.HasValue)
        {
            AddCriteria(x => x.Id == branchId.Value);
        }

        Select(x => new SurveyDashboardBranchRow
        {
            BranchId = x.Id,
            BranchNameEn = x.NameEn,
            BranchNameAr = x.NameAr,
            IsActive = x.IsActive
        });
    }
}
