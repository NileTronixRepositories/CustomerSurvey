using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.CreateQuestion
{
    internal sealed class GetCurrentBranchAdminForCreateQuestionSpec
         : Specification<BranchAdmin>
    {
        public GetCurrentBranchAdminForCreateQuestionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}