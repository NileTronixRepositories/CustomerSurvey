using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchDashboard;

internal sealed class GetCurrentBranchForBranchDashboardSpec
    : Specification<Branch, CurrentBranchActorForBranchDashboardDto>
{
    public GetCurrentBranchForBranchDashboardSpec(Guid branchId)
    {
        AddCriteria(x => x.Id == branchId);

        Select(x => new CurrentBranchActorForBranchDashboardDto
        {
            BranchId = x.Id,
            BranchNameEn = x.NameEn,
            BranchNameAr = x.NameAr
        });
    }
}
