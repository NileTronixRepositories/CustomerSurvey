using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.UpdateBranch
{
    internal sealed class GetBranchForUpdateSpec : Specification<Branch>
    {
        public GetBranchForUpdateSpec(Guid branchId)
        {
            AddCriteria(x => x.Id == branchId);
        }
    }
}