using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.UpdateOperator
{
    internal sealed class GetApplicationUserForUpdateOperatorSpec
         : Specification<ApplicationUser>
    {
        public GetApplicationUserForUpdateOperatorSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.Id == applicationUserId);
        }
    }
}