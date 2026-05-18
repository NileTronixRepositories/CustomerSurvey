using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionsPagination
{
    internal sealed class GetCurrentSuperAdminForGetGlobalQuestionsPaginationSpec
         : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForGetGlobalQuestionsPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}