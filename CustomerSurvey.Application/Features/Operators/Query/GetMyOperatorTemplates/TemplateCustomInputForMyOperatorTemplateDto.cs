using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    internal sealed record TemplateCustomInputForMyOperatorTemplateDto
    {
        public Guid CustomInputId { get; init; }

        public Guid TemplateId { get; init; }

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
