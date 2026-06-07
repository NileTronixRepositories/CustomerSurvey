using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;

namespace CustomerSurvey.Application.Features.BranchAreas.Query.GetBranchAreasPagination
{
    public sealed class GetBranchAreasPaginationQuery
        : SearchParameters, IQuery<Pagination<BranchAreaPaginationItemResponse>>
    {
    }
}
