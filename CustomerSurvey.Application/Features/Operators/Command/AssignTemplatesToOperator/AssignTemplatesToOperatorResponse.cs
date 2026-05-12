using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.AssignTemplatesToOperator
{
    public sealed record AssignTemplatesToOperatorResponse
    {
        public Guid OperatorId { get; init; }

        public int AssignedTemplatesCount { get; init; }
    }
}