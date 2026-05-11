using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetMyBranchUserRoles
{
    internal sealed class GetRolesForMyBranchUserSpec
          : Specification<UserRole, MyBranchUserRoleResponse>
    {
        public GetRolesForMyBranchUserSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            AddOrderBy(x => x.Role.Name);

            Select(x => new MyBranchUserRoleResponse
            {
                RoleId = x.RoleId,
                Name = x.Role.Name
            });
        }
    }
}