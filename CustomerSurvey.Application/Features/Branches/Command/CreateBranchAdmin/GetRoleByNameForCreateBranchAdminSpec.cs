using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.CreateBranchAdmin
{
    internal sealed class GetRoleByNameForCreateBranchAdminSpec
        : Specification<Role>
    {
        public GetRoleByNameForCreateBranchAdminSpec(string roleName)
        {
            var normalizedRoleName = roleName.Trim();

            AddCriteria(x => x.Name == normalizedRoleName);
        }
    }
}