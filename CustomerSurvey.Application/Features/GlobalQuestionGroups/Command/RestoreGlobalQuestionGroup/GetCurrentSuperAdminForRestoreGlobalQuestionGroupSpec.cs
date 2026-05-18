using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.RestoreGlobalQuestionGroup
{
    internal sealed class GetCurrentSuperAdminForRestoreGlobalQuestionGroupSpec
           : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForRestoreGlobalQuestionGroupSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}