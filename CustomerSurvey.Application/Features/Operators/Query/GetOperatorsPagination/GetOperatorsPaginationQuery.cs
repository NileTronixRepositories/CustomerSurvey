using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorsPagination
{
    public sealed class GetOperatorsPaginationQuery
        : SearchParameters, IQuery<Pagination<OperatorPaginationItemResponse>>
    {
        public Guid? DepartmentId { get; init; }
    }
}