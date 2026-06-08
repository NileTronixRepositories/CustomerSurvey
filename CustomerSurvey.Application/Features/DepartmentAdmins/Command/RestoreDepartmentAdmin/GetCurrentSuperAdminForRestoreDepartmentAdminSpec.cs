using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.DepartmentAdmins.Command.RestoreDepartmentAdmin
{
    internal sealed class GetCurrentSuperAdminForRestoreDepartmentAdminSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForRestoreDepartmentAdminSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}
