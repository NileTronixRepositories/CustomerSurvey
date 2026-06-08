using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.Departments.Command.RestoreDepartment.Specs
{
    internal sealed class GetCurrentSuperAdminForRestoreDepartmentSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForRestoreDepartmentSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}
