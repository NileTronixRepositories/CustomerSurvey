using BuildingBlock.Domain.Specification;
using CustomerSurvey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchesForSelection
{
    internal sealed class GetBranchesForSelectionSpec
          : Specification<Branch, BranchSelectionResponse>
    {
        public GetBranchesForSelectionSpec()
        {
            AddOrderBy(x => x.NameEn);

            Select(x => new BranchSelectionResponse
            {
                Id = x.Id,
                NameEn = x.NameEn,
                NameAr = x.NameAr,
                Code = x.Code
            });
        }
    }
}