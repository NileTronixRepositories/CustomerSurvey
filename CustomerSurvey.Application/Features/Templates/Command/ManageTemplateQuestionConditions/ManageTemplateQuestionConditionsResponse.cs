using CustomerSurvey.Application.Features.Templates.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions
{
    public sealed record ManageTemplateQuestionConditionsResponse
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }

        public int ConditionsCount { get; init; }

        public IReadOnlyCollection<TemplateQuestionConditionResponse> Conditions { get; init; }
            = Array.Empty<TemplateQuestionConditionResponse>();
    }
}