using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.QuestionGroups.Query.GetQuestionGroupsForSelection
{
    internal sealed class GetCurrentBranchAdminForQuestionGroupSelectionSpec
        : Specification<BranchAdmin>
    {
        public GetCurrentBranchAdminForQuestionGroupSelectionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}