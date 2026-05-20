using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate
{
    public sealed record UpdateAnonymousTemplateResponse
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

        public TemplateStatus Status { get; init; }

        public string StatusName { get; init; } = string.Empty;

        public bool IsActive { get; init; }

        public string PublicUrl { get; init; } = string.Empty;

        public string? QrCode { get; init; }

        public IReadOnlyCollection<UpdateAnonymousTemplateCustomInputResponse> CustomInputs { get; init; }
            = Array.Empty<UpdateAnonymousTemplateCustomInputResponse>();
    }

    public sealed record UpdateAnonymousTemplateCustomInputResponse
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
}