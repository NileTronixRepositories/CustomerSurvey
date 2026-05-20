using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.CreateAnonymousTemplate
{
    public sealed record CreateAnonymousTemplateCommand
        : ICommand<CreateAnonymousTemplateResponse>
    {
        public AnonymousTemplateScope? Scope { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public IReadOnlyCollection<CreateAnonymousTemplateCustomInputCommandItem> CustomInputs { get; init; }
            = Array.Empty<CreateAnonymousTemplateCustomInputCommandItem>();
    }

    public sealed record CreateAnonymousTemplateCustomInputCommandItem
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

        public int Order { get; init; }
    }
}