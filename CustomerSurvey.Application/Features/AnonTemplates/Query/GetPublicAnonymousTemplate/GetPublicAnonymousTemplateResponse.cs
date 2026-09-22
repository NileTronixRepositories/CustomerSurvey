using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonTemplates.Query.GetPublicAnonymousTemplate
{
    public sealed record GetPublicAnonymousTemplateResponse
    {
        public Guid AnonymousTemplateId { get; init; }

        public Guid? BranchId { get; init; }

        public AnonymousTemplateScope Scope { get; init; }

        public string ScopeName { get; init; } = string.Empty;

        public bool IsGlobal { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public string? LogoPath { get; init; }

        public PublicAnonymousTemplateBranchResponse? Branch { get; init; }

        public IReadOnlyCollection<PublicAnonymousTemplateCustomInputResponse> CustomInputs { get; init; }
            = Array.Empty<PublicAnonymousTemplateCustomInputResponse>();

        public IReadOnlyCollection<PublicAnonymousTemplateQuestionResponse> Questions { get; init; }
            = Array.Empty<PublicAnonymousTemplateQuestionResponse>();

        public IReadOnlyCollection<PublicAnonymousTemplateQuestionConditionResponse> QuestionConditions { get; init; }
            = Array.Empty<PublicAnonymousTemplateQuestionConditionResponse>();

        public IReadOnlyCollection<Guid> RootAnonymousTemplateQuestionIds { get; init; }
            = Array.Empty<Guid>();
    }

    public sealed record PublicAnonymousTemplateBranchResponse
    {
        public Guid BranchId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }
    }

    public sealed record PublicAnonymousTemplateCustomInputResponse
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

        public string? StartWith { get; init; }

        public int Order { get; init; }
    }

    public sealed record PublicAnonymousTemplateQuestionResponse
    {
        public Guid AnonymousTemplateQuestionId { get; init; }

        public Guid QuestionId { get; init; }

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

        public bool IsRoot { get; init; }

        public IReadOnlyCollection<PublicAnonymousTemplateQuestionOptionResponse> Options { get; init; }
            = Array.Empty<PublicAnonymousTemplateQuestionOptionResponse>();
    }

    public sealed record PublicAnonymousTemplateQuestionOptionResponse
    {
        public Guid OptionId { get; init; }

        public Guid QuestionId { get; init; }

        public string TextEn { get; init; } = string.Empty;

        public string? TextAr { get; init; }

        public int Order { get; init; }

        public int Value { get; init; }
    }

    public sealed record PublicAnonymousTemplateQuestionConditionResponse
    {
        public Guid ConditionId { get; init; }

        public Guid ParentAnonymousTemplateQuestionId { get; init; }

        public Guid ChildAnonymousTemplateQuestionId { get; init; }

        public QuestionConditionTriggerType TriggerType { get; init; }

        public string TriggerTypeName { get; init; } = string.Empty;

        public Guid? SelectedQuestionOptionId { get; init; }

        public int? TriggerValue { get; init; }

        public int Order { get; init; }
    }
}
