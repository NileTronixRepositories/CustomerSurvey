using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.GlobalQuestionGroups.Query.GetGlobalQuestionGroupsPagination
{
    internal sealed class GetCurrentSuperAdminForGetGlobalQuestionGroupsPaginationSpec
        : Specification<SuperAdmin>
    {
        public GetCurrentSuperAdminForGetGlobalQuestionGroupsPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}