using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.DeleteGlobalQuestionGroup
{
    internal sealed class GetCurrentSuperAdminForDeleteGlobalQuestionGroupSpec
         : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForDeleteGlobalQuestionGroupSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}