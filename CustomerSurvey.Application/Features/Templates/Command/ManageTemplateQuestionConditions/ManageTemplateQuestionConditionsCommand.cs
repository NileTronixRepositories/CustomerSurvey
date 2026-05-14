using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Command.ManageTemplateQuestionConditions
{
    public sealed record ManageTemplateQuestionConditionsCommand
       : ICommand<ManageTemplateQuestionConditionsResponse>
    {
        public Guid TemplateId { get; init; }

        public IReadOnlyCollection<TemplateQuestionConditionCommandItem> Conditions { get; init; }
            = Array.Empty<TemplateQuestionConditionCommandItem>();
    }

    public sealed record TemplateQuestionConditionCommandItem
    {
        public Guid ParentTemplateQuestionId { get; init; }

        public Guid ChildTemplateQuestionId { get; init; }

        public QuestionConditionTriggerType TriggerType { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? TriggerValue { get; init; }

        public int Order { get; init; }
    }
}