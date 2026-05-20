using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed class GetCurrentBranchUserForBranchDashboardSpec
    : Specification<BranchUser, CurrentBranchActorForBranchDashboardDto>
{
    public GetCurrentBranchUserForBranchDashboardSpec(Guid applicationUserId)
    {
        AddCriteria(x => x.ApplicationUserId == applicationUserId);

        Select(x => new CurrentBranchActorForBranchDashboardDto
        {
            BranchId = x.BranchId,
            BranchNameEn = x.Branch.NameEn,
            BranchNameAr = x.Branch.NameAr
        });
    }
}