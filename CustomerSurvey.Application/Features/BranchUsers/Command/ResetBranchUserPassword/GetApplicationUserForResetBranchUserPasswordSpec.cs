using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Enums;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.BranchUsers.Command.ResetBranchUserPassword
{
    internal sealed class GetApplicationUserForResetBranchUserPasswordSpec
        : Specification<ApplicationUser>
    {
        public GetApplicationUserForResetBranchUserPasswordSpec(Guid applicationUserId)
        {
            AddCriteria(x =>
                x.Id == applicationUserId &&
                x.UserType == UserType.BranchUser);
        }
    }
}