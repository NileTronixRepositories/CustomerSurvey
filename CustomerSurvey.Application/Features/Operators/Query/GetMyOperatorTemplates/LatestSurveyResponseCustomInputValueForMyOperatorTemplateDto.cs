using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    internal sealed record LatestSurveyResponseCustomInputValueForMyOperatorTemplateDto
    {
        public Guid SurveyResponseId { get; init; }

        public Guid CustomInputId { get; init; }

        public string? LabelEnSnapshot { get; init; }

        public string? LabelArSnapshot { get; init; }

        public TemplateCustomInputType Type { get; init; }

        public string? StringValue { get; init; }

        public int? IntegerValue { get; init; }
    }
}
