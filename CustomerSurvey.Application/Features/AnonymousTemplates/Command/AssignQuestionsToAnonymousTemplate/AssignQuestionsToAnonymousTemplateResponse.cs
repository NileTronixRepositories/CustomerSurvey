using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.AssignQuestionsToAnonymousTemplate
{
    public sealed record AssignQuestionsToAnonymousTemplateResponse
    {
        public Guid AnonymousTemplateId { get; init; }

        public Guid? BranchId { get; init; }

        public AnonymousTemplateScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public int AssignedQuestionsCount { get; init; }

        public IReadOnlyCollection<AssignedAnonymousTemplateQuestionResponse> Questions { get; init; }
            = Array.Empty<AssignedAnonymousTemplateQuestionResponse>();
    }

    public sealed record AssignedAnonymousTemplateQuestionResponse
    {
        public Guid AnonymousTemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public Guid? BranchId { get; init; }

        public Guid GroupId { get; init; }

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public int Order { get; init; }
    }
}