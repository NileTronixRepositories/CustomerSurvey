using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Shared.Specs
{
    internal sealed class GetCurrentBranchUserForQuestionGroupSpec
      : Specification<BranchUser, CurrentQuestionGroupBranchActorDto>
    {
        public GetCurrentBranchUserForQuestionGroupSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentQuestionGroupBranchActorDto
            {
                BranchId = x.BranchId
            });
        }
    }
}