using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    public sealed record ManageAnonymousTemplateQuestionConditionsCommand
        : ICommand<ManageAnonymousTemplateQuestionConditionsResponse>
    {
        public Guid AnonymousTemplateId { get; init; }

        public IReadOnlyCollection<ManageAnonymousTemplateQuestionConditionCommandItem> Conditions { get; init; }
            = Array.Empty<ManageAnonymousTemplateQuestionConditionCommandItem>();
    }

    public sealed record ManageAnonymousTemplateQuestionConditionCommandItem
    {
        public Guid ParentAnonymousTemplateQuestionId { get; init; }

        public Guid ChildAnonymousTemplateQuestionId { get; init; }

        public QuestionConditionTriggerType TriggerType { get; init; }

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? TriggerValue { get; init; }

        public int Order { get; init; }
    }
}