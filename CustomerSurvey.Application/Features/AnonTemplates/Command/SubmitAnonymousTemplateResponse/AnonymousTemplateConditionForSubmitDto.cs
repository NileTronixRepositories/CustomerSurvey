using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed record AnonymousTemplateConditionForSubmitDto
    {
        public Guid ConditionId { get; init; }

        public Guid ParentAnonymousTemplateQuestionId { get; init; }

        public Guid ChildAnonymousTemplateQuestionId { get; init; }

        public QuestionConditionTriggerType TriggerType { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? TriggerValue { get; init; }

        public int Order { get; init; }

        public bool IsActive { get; init; }
    }
}