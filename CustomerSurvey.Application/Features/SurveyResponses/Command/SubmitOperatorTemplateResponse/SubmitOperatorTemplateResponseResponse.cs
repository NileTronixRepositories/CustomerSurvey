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

        public int AnswersCount { get; init; }

        public DateTime SubmittedOnUtc { get; init; }
    }
}