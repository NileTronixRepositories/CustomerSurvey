using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;

namespace CustomerSurvey.Application.Features.Reports.Query.GetSystemDashboard;

internal sealed class GetSystemDashboardBranchesSpec
    : Specification<Branch, SystemDashboardBranchDto>
{
    public GetSystemDashboardBranchesSpec(Guid? branchId)
    {
        if (branchId.HasValue)
        {
            AddCriteria(x => x.Id == branchId.Value);
        }

        Select(x => new SystemDashboardBranchDto
        {
            BranchId = x.Id,
            NameEn = x.NameEn,
            NameAr = x.NameAr,
            Code = x.Code,
            IsActive = x.IsActive
        });
    }
}