using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Api.Contracts.AnonymousTemplates
{
    public sealed record CreateAnonymousTemplateRequest
    {
        public AnonymousTemplateScope? Scope { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public IReadOnlyCollection<CreateAnonymousTemplateCustomInputRequest>? CustomInputs { get; init; }
    }

    public sealed record CreateAnonymousTemplateCustomInputRequest
    {
        public string Name { get; init; } = string.Empty;

        public string? LabelEn { get; init; }

        public string? LabelAr { get; init; }

        public TemplateCustomInputType Type { get; init; }

        public bool IsRequired { get; init; }

        public int? MinLength { get; init; }

        public int? MaxLength { get; init; }

        public int? MinValue { get; init; }

        public int? MaxValue { get; init; }

        public string? StartWith { get; init; }

        public int Order { get; init; }
    }
}
