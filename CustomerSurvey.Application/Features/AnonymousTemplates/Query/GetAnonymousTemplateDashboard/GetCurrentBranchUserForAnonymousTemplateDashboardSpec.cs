using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDashboard;

internal sealed class GetCurrentBranchUserForAnonymousTemplateDashboardSpec
    : Specification<BranchUser, CurrentBranchActorForAnonymousTemplateDashboardDto>
{
    public GetCurrentBranchUserForAnonymousTemplateDashboardSpec(Guid applicationUserId)
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
