using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Command.RestoreQuestion
{
    internal sealed class GetCurrentBranchUserForRestoreQuestionSpec
       : Specification<BranchUser, CurrentBranchActorForRestoreQuestionDto>
    {
        public GetCurrentBranchUserForRestoreQuestionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForRestoreQuestionDto
            {
                BranchId = x.BranchId
            });
        }
    }
}