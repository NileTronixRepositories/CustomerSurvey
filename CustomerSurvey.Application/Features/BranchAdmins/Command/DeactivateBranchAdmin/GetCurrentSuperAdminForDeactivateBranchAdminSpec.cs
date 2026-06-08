using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAdmins.Command.DeactivateBranchAdmin
{
    internal sealed class GetCurrentSuperAdminForDeactivateBranchAdminSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForDeactivateBranchAdminSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}
