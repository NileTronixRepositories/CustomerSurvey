using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Reports.Query.GetBranchSatisfactionReport
{
    internal sealed class GetCurrentBranchUserForBranchSatisfactionReportSpec
         : Specification<BranchUser, CurrentBranchActorForBranchSatisfactionReportDto>
    {
        public GetCurrentBranchUserForBranchSatisfactionReportSpec(Guid applicationUserId)
        {
            AddCriteria(x => x.ApplicationUserId == applicationUserId);

            Select(x => new CurrentBranchActorForBranchSatisfactionReportDto
            {
                BranchId = x.BranchId
            });
        }
    }
}