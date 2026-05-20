namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.ManageAnonymousTemplateQuestionConditions
{
    internal sealed record QuestionOptionForAnonymousTemplateConditionDto
    {
        public Guid OptionId { get; init; }

        public Guid QuestionId { get; init; }

        public bool IsActive { get; init; }
    }
}