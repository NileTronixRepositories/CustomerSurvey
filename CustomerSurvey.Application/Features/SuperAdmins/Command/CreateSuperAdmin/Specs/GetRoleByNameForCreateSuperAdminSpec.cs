using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;

namespace CustomerSurvey.Application.Features.SuperAdmins.Command.CreateSuperAdmin.Specs
{
    internal sealed class GetRoleByNameForCreateSuperAdminSpec
        : Specification<Role>
    {
        public GetRoleByNameForCreateSuperAdminSpec(string roleName)
        {
            var normalizedRoleName = roleName.Trim();

            AddCriteria(x => x.Name == normalizedRoleName);
        }
    }
}
