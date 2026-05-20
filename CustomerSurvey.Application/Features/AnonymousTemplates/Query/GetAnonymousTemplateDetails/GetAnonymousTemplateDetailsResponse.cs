using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Query.GetAnonymousTemplateDetails
{
    public sealed record GetAnonymousTemplateDetailsResponse
    {
        public Guid AnonymousTemplateId { get; init; }

        public Guid? BranchId { get; init; }

        public string? BranchNameEn { get; init; }

        public string? BranchNameAr { get; init; }

        public AnonymousTemplateScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public TemplateStatus Status { get; init; }

        public string StatusName { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public string PublicUrl { get; init; } = string.Empty;

        public string? QrCode { get; init; }

        public Guid CreatedByApplicationUserId { get; init; }

        public DateTime CreatedOnUtc { get; init; }

        public DateTime? ModifiedOnUtc { get; init; }

        public AnonymousTemplateDetailsSummaryResponse Summary { get; init; } = new();

        public IReadOnlyCollection<AnonymousTemplateDetailsCustomInputResponse> CustomInputs { get; init; }
            = Array.Empty<AnonymousTemplateDetailsCustomInputResponse>();

        public IReadOnlyCollection<AnonymousTemplateDetailsQuestionResponse> Questions { get; init; }
            = Array.Empty<AnonymousTemplateDetailsQuestionResponse>();

        public IReadOnlyCollection<AnonymousTemplateDetailsQuestionConditionResponse> QuestionConditions { get; init; }
            = Array.Empty<AnonymousTemplateDetailsQuestionConditionResponse>();
    }

    public sealed record AnonymousTemplateDetailsSummaryResponse
    {
        public int QuestionsCount { get; init; }

        public int CustomInputsCount { get; init; }

        public int QuestionConditionsCount { get; init; }
    }

    public sealed record AnonymousTemplateDetailsCustomInputResponse
    {
        public Guid CustomInputId { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? LabelEn { get; init; }

        public string? LabelAr { get; init; }

        public TemplateCustomInputType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public bool IsRequired { get; init; }

        public int? MinLength { get; init; }

        public int? MaxLength { get; init; }

        public int? MinValue { get; init; }

        public int? MaxValue { get; init; }

        public int Order { get; init; }

        public bool IsActive { get; init; }
    }

    public sealed record AnonymousTemplateDetailsQuestionResponse
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

        public bool IsEditable { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public QuestionType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public int Order { get; init; }

        public bool IsActive { get; init; }

        public IReadOnlyCollection<AnonymousTemplateDetailsQuestionOptionResponse> Options { get; init; }
            = Array.Empty<AnonymousTemplateDetailsQuestionOptionResponse>();
    }

    public sealed record AnonymousTemplateDetailsQuestionOptionResponse
    {
        public Guid OptionId { get; init; }

        public Guid QuestionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        public int Value { get; init; }

        public bool IsActive { get; init; }
    }

    public sealed record AnonymousTemplateDetailsQuestionConditionResponse
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