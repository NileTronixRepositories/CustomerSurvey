using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAreas.Shared.Specs
{
    internal sealed class GetCurrentSuperAdminForBranchAreaManagementSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForBranchAreaManagementSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}
