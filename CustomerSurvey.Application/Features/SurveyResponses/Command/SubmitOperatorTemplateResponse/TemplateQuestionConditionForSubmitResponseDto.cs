using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    internal sealed record TemplateQuestionConditionForSubmitResponseDto
    {
        public Guid ConditionId { get; init; }

        public Guid TemplateId { get; init; }

        public Guid ParentTemplateQuestionId { get; init; }

        public Guid ChildTemplateQuestionId { get; init; }

        public QuestionConditionTriggerType TriggerType { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? TriggerValue { get; init; }

        public int Order { get; init; }
    }
}