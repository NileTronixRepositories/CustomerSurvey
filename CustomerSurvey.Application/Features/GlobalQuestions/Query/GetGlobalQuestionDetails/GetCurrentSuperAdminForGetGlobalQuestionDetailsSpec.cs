using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestions.Query.GetGlobalQuestionDetails
{
    internal sealed class GetCurrentSuperAdminForGetGlobalQuestionDetailsSpec
      : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForGetGlobalQuestionDetailsSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}