using CustomerSurvey.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.SurveyResponses.Command.SubmitOperatorTemplateResponse
{
    public sealed record SubmitOperatorTemplateResponseResponse
    {
        public Guid SurveyResponseId { get; init; }

        public Guid OperatorId { get; init; }

        public Guid TemplateId { get; init; }

        public int CustomInputsCount { get; init; }

        public int AnswersCount { get; init; }

        public int ActualScore { get; init; }

        public int MaxScore { get; init; }

        public decimal ScorePercentage { get; init; }

        public DateTime SubmittedOnUtc { get; init; }

        public IReadOnlyCollection<SubmittedCustomInputValueResponse> CustomInputs { get; init; }
            = Array.Empty<SubmittedCustomInputValueResponse>();
    }

    public sealed record SubmittedCustomInputValueResponse
    {
        public Guid CustomInputId { get; init; }

        public string Name { get; init; } = string.Empty;

        public TemplateCustomInputType Type { get; init; }

        public string TypeName { get; init; } = string.Empty;

        public string? StringValue { get; init; }

        public int? IntegerValue { get; init; }
    }
}