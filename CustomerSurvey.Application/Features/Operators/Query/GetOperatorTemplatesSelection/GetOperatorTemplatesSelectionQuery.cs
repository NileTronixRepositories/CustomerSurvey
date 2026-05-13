using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetOperatorTemplatesSelection
{
    public sealed record GetOperatorTemplatesSelectionQuery
         : IQuery<GetOperatorTemplatesSelectionResponse>
    {
        public Guid OperatorId { get; init; }

        public string? SearchText { get; init; }
    }
}