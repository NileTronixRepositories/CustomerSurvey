using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsForSelection
{
    internal sealed class GetCurrentBranchUserForQuestionGroupSelectionSpec
        : Specification<BranchUser>
    {
        public GetCurrentBranchUserForQuestionGroupSelectionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}