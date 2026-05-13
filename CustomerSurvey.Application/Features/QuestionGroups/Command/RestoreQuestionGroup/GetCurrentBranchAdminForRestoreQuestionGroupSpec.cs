using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Command.RestoreQuestionGroup
{
    internal sealed class GetCurrentBranchAdminForRestoreQuestionGroupSpec
         : Specification<BranchAdmin, CurrentBranchActorForRestoreQuestionGroupDto>
    {
        public GetCurrentBranchAdminForRestoreQuestionGroupSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForRestoreQuestionGroupDto
            {
                BranchId = x.BranchId
            });
        }
    }
}