using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Questions.Query.GetQuestionsPagination
{
    internal sealed class GetCurrentBranchAdminForQuestionPaginationSpec
        : Specification<BranchAdmin>
    {
        public GetCurrentBranchAdminForQuestionPaginationSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);
        }
    }
}