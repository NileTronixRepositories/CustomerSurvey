using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.UpdateQuestion
{
    internal sealed class GetCurrentBranchAdminForUpdateQuestionSpec
         : Specification<BranchAdmin>
    {
        public GetCurrentBranchAdminForUpdateQuestionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}