using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.UpdateGlobalQuestionGroup
{
    internal sealed class GetCurrentSuperAdminForUpdateGlobalQuestionGroupSpec
       : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForUpdateGlobalQuestionGroupSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}