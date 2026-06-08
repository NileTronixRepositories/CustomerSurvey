using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.DepartmentAdmins.Command.DeactivateDepartmentAdmin
{
    internal sealed class GetCurrentSuperAdminForDeactivateDepartmentAdminSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForDeactivateDepartmentAdminSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}
