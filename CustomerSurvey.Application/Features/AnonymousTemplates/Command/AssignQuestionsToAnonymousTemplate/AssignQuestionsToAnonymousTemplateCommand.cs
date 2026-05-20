using BuildingBlock.Application.Abstraction;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate
{
    public sealed record AssignQuestionsToAnonymousTemplateCommand
        : ICommand<AssignQuestionsToAnonymousTemplateResponse>
    {
        public Guid AnonymousTemplateId { get; init; }

        public IReadOnlyCollection<AssignQuestionToAnonymousTemplateCommandItem> Questions { get; init; }
            = Array.Empty<AssignQuestionToAnonymousTemplateCommandItem>();
    }

    public sealed record AssignQuestionToAnonymousTemplateCommandItem
    {
        public Guid QuestionId { get; init; }

        public int Order { get; init; }
    }
}