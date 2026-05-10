using BuildingBlock.Application.Abstraction;
using BuildingBlock.Domain.SharedDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentsPagination
{
    public sealed class GetDepartmentsPaginationQuery
         : SearchParameters, IQuery<Pagination<DepartmentPaginationItemResponse>>
    {
    }
}