using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Departments.Query.GetDepartmentDetails
{
    public sealed record GetDepartmentDetailsQuery
         : IQuery<GetDepartmentDetailsResponse>
    {
        public Guid DepartmentId { get; init; }
    }
}