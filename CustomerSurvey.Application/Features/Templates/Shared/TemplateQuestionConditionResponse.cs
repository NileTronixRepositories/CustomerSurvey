using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Templates.Shared
{
    public sealed record TemplateQuestionConditionResponse
    {
        public Guid ConditionId { get; init; }

        public Guid ParentTemplateQuestionId { get; init; }

        public Guid ChildTemplateQuestionId { get; init; }

        // 1 = SingleChoiceOption
        // 2 = StarRatingValue
        // 3 = SmileValue
        public int TriggerType { get; init; }

        public string TriggerTypeName { get; init; } = string.Empty;

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? TriggerValue { get; init; }

        public int Order { get; init; }
    }
}