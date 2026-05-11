using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.DeleteQuestion
{
    internal sealed class GetCurrentBranchUserForDeleteQuestionSpec
         : Specification<BranchUser>
    {
        public GetCurrentBranchUserForDeleteQuestionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}