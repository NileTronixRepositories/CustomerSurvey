using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Api.Contracts.AnonymousTemplates
{
    public sealed record ManageAnonymousTemplateQuestionConditionsRequest
    {
        public IReadOnlyCollection<ManageAnonymousTemplateQuestionConditionRequestItem> Conditions { get; init; }
            = Array.Empty<ManageAnonymousTemplateQuestionConditionRequestItem>();
    }

    public sealed record ManageAnonymousTemplateQuestionConditionRequestItem
    {
        public Guid ParentAnonymousTemplateQuestionId { get; init; }

        public Guid ChildAnonymousTemplateQuestionId { get; init; }

        public QuestionConditionTriggerType TriggerType { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? TriggerValue { get; init; }

        public int Order { get; init; }
    }
}