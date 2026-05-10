using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Command.CreateBranch
{
    internal sealed class BranchCodeExistsForCreateBranchSpec
     : Specification<Branch>
    {
        public BranchCodeExistsForCreateBranchSpec(string code)
        {
            var normalizedCode = code.Trim();

            AddCriteria(x => x.Code == normalizedCode);
        }
    }
}