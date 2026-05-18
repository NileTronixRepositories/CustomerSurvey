using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Command.CreateGlobalQuestionGroup
{
    internal sealed class GetCurrentSuperAdminForCreateGlobalQuestionGroupSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForCreateGlobalQuestionGroupSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}