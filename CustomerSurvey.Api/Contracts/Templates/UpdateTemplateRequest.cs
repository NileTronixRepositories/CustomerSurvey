using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Api.Contracts.Templates
{
    public sealed class UpdateTemplateRequest
    {
        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public IReadOnlyCollection<UpdateTemplateCustomInputRequest> CustomInputs { get; init; }
            = Array.Empty<UpdateTemplateCustomInputRequest>();
    }

    public sealed class UpdateTemplateCustomInputRequest
    {
        public Guid? CustomInputId { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? LabelEn { get; init; }

        public string? LabelAr { get; init; }

        public TemplateCustomInputType Type { get; init; }

        public bool IsRequired { get; init; } = true;

        public int? MinLength { get; init; }

        public int? MaxLength { get; init; }

        public int? MinValue { get; init; }

        public int? MaxValue { get; init; }

        public int Order { get; init; }
    }
}