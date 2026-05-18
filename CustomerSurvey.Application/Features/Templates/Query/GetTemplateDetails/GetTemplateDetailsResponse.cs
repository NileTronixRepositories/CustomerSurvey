using CustomerSurvey.Application.Features.Templates.Shared;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.Templates.Query.GetTemplateDetails
{
    public sealed record GetTemplateDetailsResponse
    {
        public Guid TemplateId { get; init; }

        public Guid BranchId { get; init; }

        public string BranchNameEn { get; init; } = string.Empty;

        public string? BranchNameAr { get; init; }

        public string BranchCode { get; init; } = string.Empty;

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public string Status { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public DateTime CreatedOnUtc { get; init; }

        public DateTime? ModifiedOnUtc { get; init; }

        public TemplateDetailsSummaryResponse Summary { get; init; } = new();

        public IReadOnlyCollection<TemplateDetailsQuestionResponse> Questions { get; init; }
            = Array.Empty<TemplateDetailsQuestionResponse>();

        public IReadOnlyCollection<TemplateQuestionConditionResponse> QuestionConditions { get; init; }
            = Array.Empty<TemplateQuestionConditionResponse>();
    }

    public sealed record TemplateDetailsSummaryResponse
    {
        public int QuestionsCount { get; init; }

        public int GroupsCount { get; init; }
    }

    public sealed record TemplateDetailsQuestionResponse
    {
        public Guid TemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

        public Guid? QuestionBranchId { get; init; }

        public Guid GroupId { get; init; }

        public Guid? GroupBranchId { get; init; }

        public QuestionScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public bool IsEditable { get; init; }

        public int Order { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public string Type { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public string GroupNameEn { get; init; } = string.Empty;

        public string? GroupNameAr { get; init; }

        public IReadOnlyCollection<TemplateDetailsQuestionOptionResponse> Options { get; init; }
            = Array.Empty<TemplateDetailsQuestionOptionResponse>();
    }

    public sealed record TemplateDetailsQuestionOptionResponse
    {
        public Guid OptionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        public int Value { get; init; }
    }
}