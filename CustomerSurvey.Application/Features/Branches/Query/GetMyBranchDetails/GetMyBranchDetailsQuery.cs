using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Application.Features.Branches.Query.GetBranchDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetMyBranchDetails
{
    public sealed record GetMyBranchDetailsQuery
    : IQuery<GetBranchDetailsResponse>;
}