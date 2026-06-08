using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.BranchAdmins.Command.RestoreBranchAdmin
{
    internal sealed class GetCurrentSuperAdminForRestoreBranchAdminSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForRestoreBranchAdminSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}
