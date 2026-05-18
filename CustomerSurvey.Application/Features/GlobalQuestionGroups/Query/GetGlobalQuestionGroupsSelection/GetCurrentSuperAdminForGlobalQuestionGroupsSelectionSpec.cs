using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsSelection
{
    internal sealed class GetCurrentSuperAdminForGlobalQuestionGroupsSelectionSpec
          : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForGlobalQuestionGroupsSelectionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}