using BuildingBlock.Application.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Command.AssignTemplatesToOperator
{
    public sealed record AssignTemplatesToOperatorCommand
         : ICommand<AssignTemplatesToOperatorResponse>
    {
        public Guid OperatorId { get; init; }

        public IReadOnlyCollection<Guid> TemplateIds { get; init; }
            = Array.Empty<Guid>();
    }
}