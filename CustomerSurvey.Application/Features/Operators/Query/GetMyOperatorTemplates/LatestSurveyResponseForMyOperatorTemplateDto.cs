using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSurvey.Application.Features.Operators.Query.GetMyOperatorTemplates
{
    internal sealed record LatestSurveyResponseForMyOperatorTemplateDto
    {
        public Guid SurveyResponseId { get; init; }

        public Guid TemplateId { get; init; }

        public DateTime SubmittedOnUtc { get; init; }
    }
}