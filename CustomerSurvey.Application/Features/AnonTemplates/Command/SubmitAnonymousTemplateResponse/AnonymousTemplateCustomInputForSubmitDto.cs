using CustomerSurvey.Domain.Enums;

namespace CustomerSurvey.Application.Features.AnonTemplates.Command.SubmitAnonymousTemplateResponse
{
    internal sealed record AnonymousTemplateCustomInputForSubmitDto
    {
        public Guid CustomInputId { get; init; }

        public string Name { get; init; } = string.Empty;

        public TemplateCustomInputType Type { get; init; }

        public bool IsRequired { get; init; }

        public int? MinLength { get; init; }

        public int? MaxLength { get; init; }

        public int? MinValue { get; init; }

        public int? MaxValue { get; init; }

        public bool IsActive { get; init; }
    }
}