using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails
{
    public sealed record GetBranchDetailsQuery
        : IQuery<GetBranchDetailsResponse>
    {
        public Guid BranchId { get; init; }
    }
}