using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Query.GetMyBranchUserRoles
{
    internal sealed class GetCurrentBranchUserForMyRolesSpec
          : Specification<BranchUser, CurrentBranchUserForMyRolesDto>
    {
        public GetCurrentBranchUserForMyRolesSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchUserForMyRolesDto
            {
                BranchUserId = x.Id,
                ApplicationUserId = x.ApplicationUserId,
                BranchId = x.BranchId
            });
        }
    }
}