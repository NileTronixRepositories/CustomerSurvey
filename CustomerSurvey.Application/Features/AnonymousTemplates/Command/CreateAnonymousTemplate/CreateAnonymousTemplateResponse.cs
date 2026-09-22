using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CreateAnonymousTemplate
{
    public sealed record CreateAnonymousTemplateResponse
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

        public bool IsActive { get; init; }

        public bool IsArchived { get; init; }

        public string? LogoPath { get; init; }

        public string? PublicUrl { get; init; }

        public string? QrCode { get; init; }

        public Guid CreatedByApplicationUserId { get; init; }

        public IReadOnlyCollection<CreateAnonymousTemplateCustomInputResponse> CustomInputs { get; init; }
            = Array.Empty<CreateAnonymousTemplateCustomInputResponse>();
    }

    public sealed record CreateAnonymousTemplateCustomInputResponse
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

        public bool IsActive { get; init; }
    }
}
