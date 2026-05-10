using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.CreateBranch
{
    internal sealed class GetCurrentSuperAdminForCreateBranchSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForCreateBranchSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}