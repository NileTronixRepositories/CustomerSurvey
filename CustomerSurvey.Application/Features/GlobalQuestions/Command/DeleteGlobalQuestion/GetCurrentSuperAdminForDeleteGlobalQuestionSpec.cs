using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Command.DeleteGlobalQuestion
{
    internal sealed class GetCurrentSuperAdminForDeleteGlobalQuestionSpec
         : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForDeleteGlobalQuestionSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}