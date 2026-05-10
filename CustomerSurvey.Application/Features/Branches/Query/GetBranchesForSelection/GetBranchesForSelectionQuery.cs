using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Branches.Query.GetBranchesForSelection
{
    public sealed record GetBranchesForSelectionQuery
       : IQuery<IReadOnlyCollection<BranchSelectionResponse>>;
}