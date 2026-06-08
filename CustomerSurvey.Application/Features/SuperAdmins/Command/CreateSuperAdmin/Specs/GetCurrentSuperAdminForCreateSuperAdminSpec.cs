using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.SuperAdmins.Command.CreateSuperAdmin.Specs
{
    internal sealed class GetCurrentSuperAdminForCreateSuperAdminSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForCreateSuperAdminSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}
