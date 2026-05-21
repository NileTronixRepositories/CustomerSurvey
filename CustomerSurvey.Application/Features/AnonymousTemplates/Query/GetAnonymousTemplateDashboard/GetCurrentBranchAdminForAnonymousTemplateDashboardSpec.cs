using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed class GetCurrentBranchAdminForAnonymousTemplateDashboardSpec
    : Specification<BranchAdmin, CurrentBranchActorForAnonymousTemplateDashboardDto>
{
    public GetCurrentBranchAdminForAnonymousTemplateDashboardSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new CurrentBranchActorForAnonymousTemplateDashboardDto
        {
            BranchId = x.BranchId,
            BranchNameEn = x.Branch.NameEn,
            BranchNameAr = x.Branch.NameAr
        });
    }
}
