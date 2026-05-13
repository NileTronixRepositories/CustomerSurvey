using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.DeleteBranchUser
{
    internal sealed class GetApplicationUserForDeleteBranchUserSpec
        : Specification<ApplicationUser>
    {
        public GetApplicationUserForDeleteBranchUserSpec(Guid applicationUserId)
        {
            AddCriteria(x =>
                x.Id == applicationUserId &&
                x.UserType == UserType.BranchUser);
        }
    }
}