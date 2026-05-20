using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    public sealed record ManageAnonymousTemplateQuestionConditionsResponse
    {
        public Guid AnonymousTemplateId { get; init; }

        public Guid? BranchId { get; init; }

        public AnonymousTemplateScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public int ConditionsCount { get; init; }

        public IReadOnlyCollection<ManagedAnonymousTemplateQuestionConditionResponse> Conditions { get; init; }
            = Array.Empty<ManagedAnonymousTemplateQuestionConditionResponse>();
    }

    public sealed record ManagedAnonymousTemplateQuestionConditionResponse
    {
        public Guid ConditionId { get; init; }

        public Guid ParentAnonymousTemplateQuestionId { get; init; }

        public Guid ChildAnonymousTemplateQuestionId { get; init; }

        public QuestionConditionTriggerType TriggerType { get; init; }

        public string TriggerTypeName { get; init; } = string.Empty;

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? TriggerValue { get; init; }

        public int Order { get; init; }

        public bool IsActive { get; init; }
    }
}