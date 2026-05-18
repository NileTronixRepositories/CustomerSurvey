using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Command.AssignQuestionsToTemplate
{
    public sealed record AssignQuestionsToTemplateResponse
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }

        public int QuestionsCount { get; init; }

        public IReadOnlyCollection<AssignedTemplateQuestionResponse> Questions { get; init; }
            = Array.Empty<AssignedTemplateQuestionResponse>();
    }

    public sealed record AssignedTemplateQuestionResponse
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public Guid? QuestionBranchId { get; init; }

        public Guid GroupId { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public int Order { get; init; }
    }
}