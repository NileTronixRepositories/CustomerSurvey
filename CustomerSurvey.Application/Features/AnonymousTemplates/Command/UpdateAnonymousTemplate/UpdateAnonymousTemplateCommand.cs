using BuildingBlock.Application.Abstraction;
using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.AnonymousTemplates.Command.UpdateAnonymousTemplate
{
    public sealed record UpdateAnonymousTemplateCommand
        : ICommand<UpdateAnonymousTemplateResponse>
    {
        public Guid AnonymousTemplateId { get; init; }

        public string NameEn { get; init; } = string.Empty;

        public string? NameAr { get; init; }

        public string? Description { get; init; }

        public DateTime ActiveFrom { get; init; }

        public DateTime? ExpireTo { get; init; }

        public IReadOnlyCollection<UpdateAnonymousTemplateCustomInputCommandItem> CustomInputs { get; init; }
            = Array.Empty<UpdateAnonymousTemplateCustomInputCommandItem>();
    }

    public sealed record UpdateAnonymousTemplateCustomInputCommandItem
    {
        public Guid? CustomInputId { get; init; }

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