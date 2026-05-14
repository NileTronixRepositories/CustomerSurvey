using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Api.Contracts.Templates
{
    public sealed class ManageTemplateQuestionConditionsRequest
    {
        public IReadOnlyCollection<TemplateQuestionConditionRequest> Conditions { get; init; }
            = Array.Empty<TemplateQuestionConditionRequest>();
    }

    public sealed class TemplateQuestionConditionRequest
    {
        public Guid ParentTemplateQuestionId { get; init; }

        public Guid ChildTemplateQuestionId { get; init; }

        public QuestionConditionTriggerType TriggerType { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? TriggerValue { get; init; }

        public int Order { get; init; }
    }
}