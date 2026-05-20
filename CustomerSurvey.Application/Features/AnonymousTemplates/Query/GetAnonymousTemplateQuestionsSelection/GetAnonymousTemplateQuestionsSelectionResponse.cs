using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateQuestionsSelection
{
    public sealed record GetAnonymousTemplateQuestionsSelectionResponse
    {
        public Guid AnonymousTemplateId { get; init; }

        public Guid? BranchId { get; init; }

        public AnonymousTemplateScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public int SelectedQuestionsCount { get; init; }

        public IReadOnlyCollection<AnonymousTemplateQuestionSelectionItemResponse> Questions { get; init; }
            = Array.Empty<AnonymousTemplateQuestionSelectionItemResponse>();
    }

    public sealed record AnonymousTemplateQuestionSelectionItemResponse
    {
        public Guid QuestionId { get; init; }

        public Guid? AnonymousTemplateQuestionId { get; init; }

        public Guid? BranchId { get; init; }

        public Guid GroupId { get; init; }

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public bool IsEditable { get; init; }

        public bool IsSelected { get; init; }

        public int? SelectedOrder { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public int? Value { get; init; }

        public bool IsActive { get; init; }

        public IReadOnlyCollection<AnonymousTemplateQuestionSelectionOptionResponse> Options { get; init; }
            = Array.Empty<AnonymousTemplateQuestionSelectionOptionResponse>();
    }

    public sealed record AnonymousTemplateQuestionSelectionOptionResponse
    {
        public Guid OptionId { get; init; }

        public Guid QuestionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        public int Value { get; init; }

        public bool IsActive { get; init; }
    }
}