using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchesPagination
{
    public sealed class GetBranchesPaginationQuery
       : SearchParameters, IQuery<Pagination<BranchPaginationItemResponse>>
    {
    }
}